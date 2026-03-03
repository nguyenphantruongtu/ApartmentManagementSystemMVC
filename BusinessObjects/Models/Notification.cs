using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Notification
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string Type { get; set; } = null!;

    public int UserId { get; set; }

    public int? SenderId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public int? RelatedInvoiceId { get; set; }

    public int? RelatedIssueId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Invoice? RelatedInvoice { get; set; }

    public virtual Issue? RelatedIssue { get; set; }

    public virtual User? Sender { get; set; }

    public virtual User User { get; set; } = null!;
}
