using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dms.Core.Entities;

namespace Dms.Core.Interfaces;

public interface IDocumentRepository
{
    Task<Document> GetByIdAsync(Guid id);
    Task<IEnumerable<Document>> GetAllAsync();
    Task AddAsync(Document document);
    Task UpdateAsync(Document document);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<DocumentVersion>> GetVersionsAsync(Guid documentId);
    Task<DocumentVersion> GetVersionByIdAsync(Guid id);
    Task AddVersionAsync(DocumentVersion version);
    Task<IEnumerable<DocumentMetadata>> GetMetadataAsync(Guid documentId);
    Task AddMetadataAsync(DocumentMetadata metadata);
    Task UpdateMetadataAsync(DocumentMetadata metadata);
    Task DeleteMetadataAsync(Guid id);
    Task<IEnumerable<DocumentPermission>> GetPermissionsAsync(Guid documentId);
    Task AddPermissionAsync(DocumentPermission permission);
    Task UpdatePermissionAsync(DocumentPermission permission);
    Task DeletePermissionAsync(Guid id);
}