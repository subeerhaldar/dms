using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dms.Core.Entities;

namespace Dms.Core.Interfaces;

public interface IDocumentService
{
    Task<Document> GetDocumentAsync(Guid id);
    Task<IEnumerable<Document>> GetAllDocumentsAsync();
    Task<Document> CreateDocumentAsync(Document document, string userId);
    Task UpdateDocumentAsync(Document document, string userId);
    Task DeleteDocumentAsync(Guid id);
    Task<DocumentVersion> CreateVersionAsync(DocumentVersion version);
    Task<IEnumerable<DocumentVersion>> GetDocumentVersionsAsync(Guid documentId);
    Task<DocumentVersion> GetVersionAsync(Guid id);
    Task RollbackToVersionAsync(Guid documentId, Guid versionId, string userId);
    Task<IEnumerable<DocumentMetadata>> GetDocumentMetadataAsync(Guid documentId);
    Task AddMetadataAsync(DocumentMetadata metadata);
    Task UpdateMetadataAsync(DocumentMetadata metadata);
    Task DeleteMetadataAsync(Guid id);
    Task<IEnumerable<DocumentPermission>> GetDocumentPermissionsAsync(Guid documentId);
    Task AddPermissionAsync(DocumentPermission permission);
    Task UpdatePermissionAsync(DocumentPermission permission);
    Task DeletePermissionAsync(Guid id);
    Task<bool> HasPermissionAsync(Guid documentId, string userId, string requiredRole);
}