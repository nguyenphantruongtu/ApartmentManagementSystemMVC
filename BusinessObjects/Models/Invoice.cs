using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Invoice
{
    public int Id { get; set; }

    public string InvoiceCode { get; set; } = null!;

    public int ApartmentId { get; set; }

    public int ResidentId { get; set; }

    public int BillingMonth { get; set; }

    public int BillingYear { get; set; }

    public DateOnly IssueDate { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Apartment Apartment { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Resident Resident { get; set; } = null!;
}
