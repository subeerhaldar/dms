using System;

namespace Dms.Core.Entities;

public class DocumentPermission
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string UserId { get; set; }
    public string Role { get; set; } // Admin, Editor, Viewer

    // Navigation property
    public Document Document { get; set; }
}