using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dms.Core.Entities;
using Dms.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dms.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Document>>> GetDocuments()
    {
        var documents = await _documentService.GetAllDocumentsAsync();
        return Ok(documents);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Document>> GetDocument(Guid id)
    {
        var document = await _documentService.GetDocumentAsync(id);
        if (document == null)
            return NotFound();

        if (!await _documentService.HasPermissionAsync(id, GetUserId(), "Viewer") &&
            !await _documentService.HasPermissionAsync(id, GetUserId(), "Editor") &&
            !await _documentService.HasPermissionAsync(id, GetUserId(), "Admin"))
            return Forbid();

        return Ok(document);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Document>> CreateDocument(IFormFile file, string fileName, int workflowStateId = 1)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var document = new Document
        {
            FileName = fileName ?? file.FileName,
            ContentType = file.ContentType,
            FileData = memoryStream.ToArray(),
            WorkflowStateId = workflowStateId
        };

        var createdDocument = await _documentService.CreateDocumentAsync(document, GetUserId());
        return CreatedAtAction(nameof(GetDocument), new { id = createdDocument.Id }, createdDocument);
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateDocument(Guid id, IFormFile file, string fileName)
    {
        var document = await _documentService.GetDocumentAsync(id);
        if (document == null)
            return NotFound();

        if (!await _documentService.HasPermissionAsync(id, GetUserId(), "Editor") &&
            !await _documentService.HasPermissionAsync(id, GetUserId(), "Admin"))
            return Forbid();

        if (file != null && file.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            document.FileData = memoryStream.ToArray();
            document.ContentType = file.ContentType;
        }

        if (!string.IsNullOrEmpty(fileName))
            document.FileName = fileName;

        await _documentService.UpdateDocumentAsync(document, GetUserId());
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        var document = await _documentService.GetDocumentAsync(id);
        if (document == null)
            return NotFound();

        if (!await _documentService.HasPermissionAsync(id, GetUserId(), "Admin"))
            return Forbid();

        await _documentService.DeleteDocumentAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocument(Guid id)
    {
        var document = await _documentService.GetDocumentAsync(id);
        if (document == null)
            return NotFound();

        if (!await _documentService.HasPermissionAsync(id, GetUserId(), "Viewer") &&
            !await _documentService.HasPermissionAsync(id, GetUserId(), "Editor") &&
            !await _documentService.HasPermissionAsync(id, GetUserId(), "Admin"))
            return Forbid();

        return File(document.FileData, document.ContentType, document.FileName);
    }

    // Metadata endpoints
    [HttpGet("{id}/metadata")]
    public async Task<ActionResult<IEnumerable<DocumentMetadata>>> GetMetadata(Guid id)
    {
        if (!await _documentService.HasPermissionAsync(id, GetUserId(), "Viewer") &&
            !await _documentService.HasPermissionAsync(id, GetUserId(), "Editor") &&
            !await _documentService.HasPermissionAsync(id, GetUserId(), "Admin"))
            return Forbid();

        var metadata = await _documentService.GetDocumentMetadataAsync(id);
        return Ok(metadata);
    }

    [HttpPost("{id}/metadata")]
    public async Task<IActionResult> AddMetadata(Guid id, [FromBody] DocumentMetadata metadata)
    {
        if (!await _documentService.HasPermissionAsync(id, GetUserId(), "Editor") &&
            !await _documentService.HasPermissionAsync(id, GetUserId(), "Admin"))
            return Forbid();

        metadata.DocumentId = id;
        await _documentService.AddMetadataAsync(metadata);
        return CreatedAtAction(nameof(GetMetadata), new { id }, metadata);
    }

    // Versions
    [HttpGet("{id}/versions")]
    public async Task<ActionResult<IEnumerable<DocumentVersion>>> GetVersions(Guid id)
    {
        if (!await _documentService.HasPermissionAsync(id, GetUserId(), "Viewer") &&
            !await _documentService.HasPermissionAsync(id, GetUserId(), "Editor") &&
            !await _documentService.HasPermissionAsync(id, GetUserId(), "Admin"))
            return Forbid();

        var versions = await _documentService.GetDocumentVersionsAsync(id);
        return Ok(versions);
    }

    [HttpPost("{id}/rollback/{versionId}")]
    public async Task<IActionResult> RollbackToVersion(Guid id, Guid versionId)
    {
        if (!await _documentService.HasPermissionAsync(id, GetUserId(), "Editor") &&
            !await _documentService.HasPermissionAsync(id, GetUserId(), "Admin"))
            return Forbid();

        await _documentService.RollbackToVersionAsync(id, versionId, GetUserId());
        return NoContent();
    }

    // Permissions
    [HttpGet("{id}/permissions")]
    public async Task<ActionResult<IEnumerable<DocumentPermission>>> GetPermissions(Guid id)
    {
        if (!await _documentService.HasPermissionAsync(id, GetUserId(), "Admin"))
            return Forbid();

        var permissions = await _documentService.GetDocumentPermissionsAsync(id);
        return Ok(permissions);
    }

    [HttpPost("{id}/permissions")]
    public async Task<IActionResult> AddPermission(Guid id, [FromBody] DocumentPermission permission)
    {
        if (!await _documentService.HasPermissionAsync(id, GetUserId(), "Admin"))
            return Forbid();

        permission.DocumentId = id;
        await _documentService.AddPermissionAsync(permission);
        return CreatedAtAction(nameof(GetPermissions), new { id }, permission);
    }
}