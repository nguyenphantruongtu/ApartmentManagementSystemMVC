using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class IssueCategory
{
    public int Id { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public int PriorityLevel { get; set; }

    public int EstimatedResolutionDays { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();
}
