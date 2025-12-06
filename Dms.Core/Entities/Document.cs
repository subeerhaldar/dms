using System;
using System.Collections.Generic;

namespace Dms.Core.Entities;

public class Document
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public byte[] FileData { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }

    // Navigation properties
    public ICollection<DocumentVersion> Versions { get; set; }
    public ICollection<DocumentMetadata> Metadata { get; set; }
    public ICollection<DocumentPermission> Permissions { get; set; }
    public int WorkflowStateId { get; set; }
    public WorkflowState WorkflowState { get; set; }
}