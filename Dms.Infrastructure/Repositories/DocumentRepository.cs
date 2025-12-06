using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dms.Core.Entities;
using Dms.Core.Interfaces;
using Dms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dms.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Document> GetByIdAsync(Guid id)
    {
        return await _context.Documents
            .Include(d => d.Versions)
            .Include(d => d.Metadata)
            .Include(d => d.Permissions)
            .Include(d => d.WorkflowState)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        return await _context.Documents
            .Include(d => d.WorkflowState)
            .ToListAsync();
    }

    public async Task AddAsync(Document document)
    {
        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Document document)
    {
        var entry = _context.Entry(document);
        if (entry.State == EntityState.Detached)
        {
            _context.Documents.Attach(document);
            entry.State = EntityState.Modified;
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var document = await GetByIdAsync(id);
        if (document != null)
        {
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<DocumentVersion>> GetVersionsAsync(Guid documentId)
    {
        return await _context.DocumentVersions
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();
    }

    public async Task<DocumentVersion> GetVersionByIdAsync(Guid id)
    {
        return await _context.DocumentVersions.FindAsync(id);
    }

    public async Task AddVersionAsync(DocumentVersion version)
    {
        await _context.DocumentVersions.AddAsync(version);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<DocumentMetadata>> GetMetadataAsync(Guid documentId)
    {
        return await _context.DocumentMetadata
            .Where(m => m.DocumentId == documentId)
            .ToListAsync();
    }

    public async Task AddMetadataAsync(DocumentMetadata metadata)
    {
        await _context.DocumentMetadata.AddAsync(metadata);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateMetadataAsync(DocumentMetadata metadata)
    {
        _context.DocumentMetadata.Update(metadata);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteMetadataAsync(Guid id)
    {
        var metadata = await _context.DocumentMetadata.FindAsync(id);
        if (metadata != null)
        {
            _context.DocumentMetadata.Remove(metadata);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<DocumentPermission>> GetPermissionsAsync(Guid documentId)
    {
        return await _context.DocumentPermissions
            .Where(p => p.DocumentId == documentId)
            .ToListAsync();
    }

    public async Task AddPermissionAsync(DocumentPermission permission)
    {
        await _context.DocumentPermissions.AddAsync(permission);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePermissionAsync(DocumentPermission permission)
    {
        _context.DocumentPermissions.Update(permission);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePermissionAsync(Guid id)
    {
        var permission = await _context.DocumentPermissions.FindAsync(id);
        if (permission != null)
        {
            _context.DocumentPermissions.Remove(permission);
            await _context.SaveChangesAsync();
        }
    }
}