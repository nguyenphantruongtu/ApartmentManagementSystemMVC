using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class InvoiceDetail
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }

    public int FeeTypeId { get; set; }

    public string? Description { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Amount { get; set; }

    public virtual FeeType FeeType { get; set; } = null!;

    public virtual Invoice Invoice { get; set; } = null!;
}
