using System.Collections.Generic;

namespace Dms.Core.Entities;

public class WorkflowState
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Navigation property
    public ICollection<Document> Documents { get; set; }
}