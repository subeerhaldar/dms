using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dms.Core.Entities;
using Dms.Core.Interfaces;

namespace Dms.Core.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;

    public DocumentService(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Document> GetDocumentAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Document>> GetAllDocumentsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Document> CreateDocumentAsync(Document document, string userId)
    {
        document.Id = Guid.NewGuid();
        document.CreatedBy = userId;
        document.CreatedDate = DateTime.UtcNow;
        document.ModifiedBy = userId;
        document.ModifiedDate = DateTime.UtcNow;
        await _repository.AddAsync(document);
        return document;
    }

    public async Task UpdateDocumentAsync(Document document, string userId)
    {
        // Create a version before updating
        var versions = await _repository.GetVersionsAsync(document.Id);
        var nextVersionNumber = versions.Any() ? versions.Max(v => v.VersionNumber) + 1 : 1;

        var version = new DocumentVersion
        {
            DocumentId = document.Id,
            VersionNumber = nextVersionNumber,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileData = document.FileData,
            CreatedBy = document.ModifiedBy ?? userId
        };
        await _repository.AddVersionAsync(version);

        document.ModifiedBy = userId;
        document.ModifiedDate = DateTime.UtcNow;
        await _repository.UpdateAsync(document);
    }

    public async Task DeleteDocumentAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<DocumentVersion> CreateVersionAsync(DocumentVersion version)
    {
        version.Id = Guid.NewGuid();
        version.CreatedDate = DateTime.UtcNow;
        await _repository.AddVersionAsync(version);
        return version;
    }

    public async Task<IEnumerable<DocumentVersion>> GetDocumentVersionsAsync(Guid documentId)
    {
        return await _repository.GetVersionsAsync(documentId);
    }

    public async Task<DocumentVersion> GetVersionAsync(Guid id)
    {
        return await _repository.GetVersionByIdAsync(id);
    }

    public async Task RollbackToVersionAsync(Guid documentId, Guid versionId, string userId)
    {
        var version = await _repository.GetVersionByIdAsync(versionId);
        if (version != null && version.DocumentId == documentId)
        {
            var document = await _repository.GetByIdAsync(documentId);
            document.FileName = version.FileName;
            document.ContentType = version.ContentType;
            document.FileData = version.FileData;
            document.ModifiedBy = userId;
            document.ModifiedDate = DateTime.UtcNow;
            await _repository.UpdateAsync(document);
        }
    }

    public async Task<IEnumerable<DocumentMetadata>> GetDocumentMetadataAsync(Guid documentId)
    {
        return await _repository.GetMetadataAsync(documentId);
    }

    public async Task AddMetadataAsync(DocumentMetadata metadata)
    {
        metadata.Id = Guid.NewGuid();
        await _repository.AddMetadataAsync(metadata);
    }

    public async Task UpdateMetadataAsync(DocumentMetadata metadata)
    {
        await _repository.UpdateMetadataAsync(metadata);
    }

    public async Task DeleteMetadataAsync(Guid id)
    {
        await _repository.DeleteMetadataAsync(id);
    }

    public async Task<IEnumerable<DocumentPermission>> GetDocumentPermissionsAsync(Guid documentId)
    {
        return await _repository.GetPermissionsAsync(documentId);
    }

    public async Task AddPermissionAsync(DocumentPermission permission)
    {
        permission.Id = Guid.NewGuid();
        await _repository.AddPermissionAsync(permission);
    }

    public async Task UpdatePermissionAsync(DocumentPermission permission)
    {
        await _repository.UpdatePermissionAsync(permission);
    }

    public async Task DeletePermissionAsync(Guid id)
    {
        await _repository.DeletePermissionAsync(id);
    }

    public async Task<bool> HasPermissionAsync(Guid documentId, string userId, string requiredRole)
    {
        var permissions = await _repository.GetPermissionsAsync(documentId);
        return permissions.Any(p => p.UserId == userId && p.Role == requiredRole);
    }
}