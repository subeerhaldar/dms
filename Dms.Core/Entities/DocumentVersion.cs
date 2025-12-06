using System;

namespace Dms.Core.Entities;

public class DocumentVersion
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public byte[] FileData { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }

    // Navigation property
    public Document Document { get; set; }
}