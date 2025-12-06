using Dms.Core.Entities;
using Dms.Core.Services;
using Dms.Core.Interfaces;
using Dms.Infrastructure.Repositories;
using Dms.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dms.Api.Tests;

[TestFixture]
public class DocumentServiceTests
{
    private ApplicationDbContext _context;
    private IDocumentRepository _repository;
    private IDocumentService _service;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        // Seed workflow states
        if (!_context.WorkflowStates.Any())
        {
            _context.WorkflowStates.AddRange(
                new WorkflowState { Id = 1, Name = "Pending" },
                new WorkflowState { Id = 2, Name = "Approved" },
                new WorkflowState { Id = 3, Name = "Rejected" }
            );
            _context.SaveChanges();
        }

        _repository = new DocumentRepository(_context);
        _service = new DocumentService(_repository);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task CreateDocumentAsync_ShouldCreateDocument_WhenValidDataProvided()
    {
        // Arrange
        var document = new Document
        {
            FileName = "test.txt",
            ContentType = "text/plain",
            FileData = System.Text.Encoding.UTF8.GetBytes("Test content"),
            WorkflowStateId = 1
        };

        // Act
        var result = await _service.CreateDocumentAsync(document, "test-user");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.FileName.Should().Be("test.txt");
        result.CreatedBy.Should().Be("test-user");
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task GetDocumentAsync_ShouldReturnDocument_WhenDocumentExists()
    {
        // Arrange
        var document = new Document
        {
            FileName = "test.txt",
            ContentType = "text/plain",
            FileData = System.Text.Encoding.UTF8.GetBytes("Test content"),
            WorkflowStateId = 1
        };
        var createdDocument = await _service.CreateDocumentAsync(document, "test-user");

        // Act
        var result = await _service.GetDocumentAsync(createdDocument.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(createdDocument.Id);
        result.FileName.Should().Be("test.txt");
    }

    [Test]
    public async Task GetDocumentAsync_ShouldReturnNull_WhenDocumentDoesNotExist()
    {
        // Act
        var result = await _service.GetDocumentAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetAllDocumentsAsync_ShouldReturnAllDocuments()
    {
        // Arrange
        var document1 = new Document
        {
            FileName = "test1.txt",
            ContentType = "text/plain",
            FileData = System.Text.Encoding.UTF8.GetBytes("Test content 1"),
            WorkflowStateId = 1
        };
        var document2 = new Document
        {
            FileName = "test2.txt",
            ContentType = "text/plain",
            FileData = System.Text.Encoding.UTF8.GetBytes("Test content 2"),
            WorkflowStateId = 1
        };

        await _service.CreateDocumentAsync(document1, "test-user");
        await _service.CreateDocumentAsync(document2, "test-user");

        // Act
        var result = await _service.GetAllDocumentsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Select(d => d.FileName).Should().Contain(new[] { "test1.txt", "test2.txt" });
    }

    [Test]
    public async Task UpdateDocumentAsync_ShouldUpdateDocumentAndCreateVersion()
    {
        // Arrange
        var document = new Document
        {
            FileName = "original.txt",
            ContentType = "text/plain",
            FileData = System.Text.Encoding.UTF8.GetBytes("Original content"),
            WorkflowStateId = 1
        };
        var createdDocument = await _service.CreateDocumentAsync(document, "test-user");

        // Update the document with new values
        var updatedDocument = new Document
        {
            Id = createdDocument.Id,
            FileName = "updated.txt",
            ContentType = createdDocument.ContentType,
            FileData = System.Text.Encoding.UTF8.GetBytes("Updated content")
        };

        // Act
        await _service.UpdateDocumentAsync(updatedDocument, "update-user");

        // Assert
        var retrievedDocument = await _service.GetDocumentAsync(createdDocument.Id);
        retrievedDocument.FileName.Should().Be("updated.txt");
        retrievedDocument.ModifiedBy.Should().Be("update-user");
        retrievedDocument.ModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Check that a version was created
        var versions = await _service.GetDocumentVersionsAsync(createdDocument.Id);
        versions.Should().HaveCount(1);
        versions.First().VersionNumber.Should().Be(1);
        versions.First().FileName.Should().Be("original.txt");
    }

    [Test]
    public async Task DeleteDocumentAsync_ShouldRemoveDocument()
    {
        // Arrange
        var document = new Document
        {
            FileName = "test.txt",
            ContentType = "text/plain",
            FileData = System.Text.Encoding.UTF8.GetBytes("Test content"),
            WorkflowStateId = 1
        };
        var createdDocument = await _service.CreateDocumentAsync(document, "test-user");

        // Act
        await _service.DeleteDocumentAsync(createdDocument.Id);

        // Assert
        var result = await _service.GetDocumentAsync(createdDocument.Id);
        result.Should().BeNull();
    }

    [Test]
    public async Task AddMetadataAsync_ShouldAddMetadataToDocument()
    {
        // Arrange
        var document = new Document
        {
            FileName = "test.txt",
            ContentType = "text/plain",
            FileData = System.Text.Encoding.UTF8.GetBytes("Test content"),
            WorkflowStateId = 1
        };
        var createdDocument = await _service.CreateDocumentAsync(document, "test-user");

        var metadata = new DocumentMetadata
        {
            Key = "Department",
            Value = "IT"
        };

        // Act
        metadata.DocumentId = createdDocument.Id;
        await _service.AddMetadataAsync(metadata);

        // Assert
        var documentMetadata = await _service.GetDocumentMetadataAsync(createdDocument.Id);
        documentMetadata.Should().HaveCount(1);
        documentMetadata.First().Key.Should().Be("Department");
        documentMetadata.First().Value.Should().Be("IT");
    }

    [Test]
    public async Task GetDocumentVersionsAsync_ShouldReturnVersionsInCorrectOrder()
    {
        // Arrange
        var document = new Document
        {
            FileName = "test.txt",
            ContentType = "text/plain",
            FileData = System.Text.Encoding.UTF8.GetBytes("Version 1"),
            WorkflowStateId = 1
        };
        var createdDocument = await _service.CreateDocumentAsync(document, "test-user");

        // Update twice to create versions
        createdDocument.FileData = System.Text.Encoding.UTF8.GetBytes("Version 2");
        await _service.UpdateDocumentAsync(createdDocument, "test-user");

        createdDocument.FileData = System.Text.Encoding.UTF8.GetBytes("Version 3");
        await _service.UpdateDocumentAsync(createdDocument, "test-user");

        // Act
        var versions = await _service.GetDocumentVersionsAsync(createdDocument.Id);

        // Assert
        versions.Should().HaveCount(2); // Two updates = two versions
        versions.First().VersionNumber.Should().Be(2); // Most recent first
        versions.Last().VersionNumber.Should().Be(1); // Oldest last
    }

    [Test]
    public async Task RollbackToVersionAsync_ShouldRestorePreviousVersion()
    {
        // Arrange
        var document = new Document
        {
            FileName = "test.txt",
            ContentType = "text/plain",
            FileData = System.Text.Encoding.UTF8.GetBytes("Version 1"),
            WorkflowStateId = 1
        };
        var createdDocument = await _service.CreateDocumentAsync(document, "test-user");

        // Update to create a version
        createdDocument.FileData = System.Text.Encoding.UTF8.GetBytes("Version 2");
        await _service.UpdateDocumentAsync(createdDocument, "test-user");

        // Get the version to rollback to
        var versions = await _service.GetDocumentVersionsAsync(createdDocument.Id);
        var versionToRollback = versions.First(v => v.VersionNumber == 1);

        // Act
        await _service.RollbackToVersionAsync(createdDocument.Id, versionToRollback.Id, "rollback-user");

        // Assert
        var rolledBackDocument = await _service.GetDocumentAsync(createdDocument.Id);
        System.Text.Encoding.UTF8.GetString(rolledBackDocument.FileData).Should().Be("Version 1");
        rolledBackDocument.ModifiedBy.Should().Be("rollback-user");
    }

    [Test]
    public async Task HasPermissionAsync_ShouldReturnTrue_WhenUserHasPermission()
    {
        // Arrange
        var document = new Document
        {
            FileName = "test.txt",
            ContentType = "text/plain",
            FileData = System.Text.Encoding.UTF8.GetBytes("Test content"),
            WorkflowStateId = 1
        };
        var createdDocument = await _service.CreateDocumentAsync(document, "test-user");

        var permission = new DocumentPermission
        {
            UserId = "test-user",
            Role = "Editor",
            DocumentId = createdDocument.Id
        };
        await _service.AddPermissionAsync(permission);

        // Act
        var hasPermission = await _service.HasPermissionAsync(createdDocument.Id, "test-user", "Editor");

        // Assert
        hasPermission.Should().BeTrue();
    }

    [Test]
    public async Task HasPermissionAsync_ShouldReturnFalse_WhenUserDoesNotHavePermission()
    {
        // Arrange
        var document = new Document
        {
            FileName = "test.txt",
            ContentType = "text/plain",
            FileData = System.Text.Encoding.UTF8.GetBytes("Test content"),
            WorkflowStateId = 1
        };
        var createdDocument = await _service.CreateDocumentAsync(document, "test-user");

        // Act
        var hasPermission = await _service.HasPermissionAsync(createdDocument.Id, "test-user", "Admin");

        // Assert
        hasPermission.Should().BeFalse();
    }

    [Test]
    public async Task AddPermissionAsync_ShouldAddPermissionToDocument()
    {
        // Arrange
        var document = new Document
        {
            FileName = "test.txt",
            ContentType = "text/plain",
            FileData = System.Text.Encoding.UTF8.GetBytes("Test content"),
            WorkflowStateId = 1
        };
        var createdDocument = await _service.CreateDocumentAsync(document, "test-user");

        var permission = new DocumentPermission
        {
            UserId = "test-user",
            Role = "Viewer",
            DocumentId = createdDocument.Id
        };

        // Act
        await _service.AddPermissionAsync(permission);

        // Assert
        var documentPermissions = await _service.GetDocumentPermissionsAsync(createdDocument.Id);
        documentPermissions.Should().HaveCount(1);
        documentPermissions.First().Role.Should().Be("Viewer");
        documentPermissions.First().UserId.Should().Be("test-user");
    }
}