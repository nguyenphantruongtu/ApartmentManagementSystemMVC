using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class FeeType
{
    public int Id { get; set; }

    public string FeeCode { get; set; } = null!;

    public string FeeName { get; set; } = null!;

    public string? Description { get; set; }

    public string? Unit { get; set; }

    public decimal UnitPrice { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
}
