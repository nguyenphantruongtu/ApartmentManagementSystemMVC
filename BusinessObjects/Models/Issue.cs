using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Issue
{
    public int Id { get; set; }

    public string IssueCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int ApartmentId { get; set; }

    public int CategoryId { get; set; }

    public int ReportedByUserId { get; set; }

    public int? AssignedToUserId { get; set; }

    public int Priority { get; set; }

    public string Status { get; set; } = null!;

    public string? ImageUrls { get; set; }

    public string? ResolutionNotes { get; set; }

    public DateTime? ResolvedDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Apartment Apartment { get; set; } = null!;

    public virtual User? AssignedToUser { get; set; }

    public virtual IssueCategory Category { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual User ReportedByUser { get; set; } = null!;
}
