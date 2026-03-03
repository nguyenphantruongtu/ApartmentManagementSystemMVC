using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Issue> IssueAssignedToUsers { get; set; } = new List<Issue>();

    public virtual ICollection<Issue> IssueReportedByUsers { get; set; } = new List<Issue>();

    public virtual ICollection<Notification> NotificationSenders { get; set; } = new List<Notification>();

    public virtual ICollection<Notification> NotificationUsers { get; set; } = new List<Notification>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<Resident> Residents { get; set; } = new List<Resident>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
