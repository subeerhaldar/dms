using System;

namespace Dms.Core.Entities;

public class DocumentMetadata
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }

    // Navigation property
    public Document Document { get; set; }
}