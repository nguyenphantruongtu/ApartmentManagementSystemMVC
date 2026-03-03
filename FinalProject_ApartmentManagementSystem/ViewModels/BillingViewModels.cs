using System.ComponentModel.DataAnnotations;

namespace FinalProject_ApartmentManagementSystem.ViewModels;

public class FeeTypesIndexViewModel
{
    public List<FeeTypeListItemViewModel> FeeTypes { get; set; } = new();
}

public class FeeTypeListItemViewModel
{
    public int Id { get; set; }
    public string FeeCode { get; set; } = string.Empty;
    public string FeeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FeeTypeFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Fee code is required.")]
    [MaxLength(50)]
    public string FeeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fee name is required.")]
    [MaxLength(255)]
    public string FeeName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    [Range(0.01, 100000000, ErrorMessage = "Unit price must be greater than 0.")]
    public decimal UnitPrice { get; set; }

    public bool IsActive { get; set; } = true;
}

public class InvoiceIndexViewModel
{
    public InvoiceFilterViewModel Filter { get; set; } = new();
    public List<InvoiceListItemViewModel> Invoices { get; set; } = new();
    public List<ApartmentOptionViewModel> ApartmentOptions { get; set; } = new();
}

public class InvoiceFilterViewModel
{
    public string? Status { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public int? ApartmentId { get; set; }
    public List<string> StatusOptions { get; set; } = new();
}

public class InvoiceListItemViewModel
{
    public int Id { get; set; }
    public string InvoiceCode { get; set; } = string.Empty;
    public string ApartmentCode { get; set; } = string.Empty;
    public string ResidentName { get; set; } = string.Empty;
    public int BillingMonth { get; set; }
    public int BillingYear { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class InvoiceCreateViewModel
{
    [Required(ErrorMessage = "Apartment is required.")]
    public int ApartmentId { get; set; }

    [Required(ErrorMessage = "Resident is required.")]
    public int ResidentId { get; set; }

    [Range(1, 12, ErrorMessage = "Billing month is invalid.")]
    public int BillingMonth { get; set; }

    [Range(2000, 2100, ErrorMessage = "Billing year is invalid.")]
    public int BillingYear { get; set; }

    [DataType(DataType.Date)]
    public DateOnly IssueDate { get; set; }

    [DataType(DataType.Date)]
    public DateOnly DueDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public List<FeeTypeQuantityViewModel> FeeItems { get; set; } = new();
    public List<ApartmentOptionViewModel> ApartmentOptions { get; set; } = new();
    public List<ResidentOptionViewModel> ResidentOptions { get; set; } = new();
}

public class FeeTypeQuantityViewModel
{
    public int FeeTypeId { get; set; }
    public string FeeName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal UnitPrice { get; set; }

    [Range(0, 1000000, ErrorMessage = "Quantity must be non-negative.")]
    public decimal Quantity { get; set; }
}

public class InvoiceDetailsViewModel
{
    public int Id { get; set; }
    public string InvoiceCode { get; set; } = string.Empty;
    public string ApartmentCode { get; set; } = string.Empty;
    public string ResidentName { get; set; } = string.Empty;
    public int BillingMonth { get; set; }
    public int BillingYear { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<InvoiceDetailLineViewModel> Details { get; set; } = new();
    public List<PaymentListItemViewModel> Payments { get; set; } = new();
}

public class InvoiceDetailLineViewModel
{
    public string FeeName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}

public class PaymentIndexViewModel
{
    public List<PaymentListItemViewModel> Payments { get; set; } = new();
}

public class PaymentListItemViewModel
{
    public int Id { get; set; }
    public string InvoiceCode { get; set; } = string.Empty;
    public string ApartmentCode { get; set; } = string.Empty;
    public string ResidentName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string? ReceivedByName { get; set; }
}

public class PaymentCreateViewModel
{
    public int InvoiceId { get; set; }
    public string InvoiceCode { get; set; } = string.Empty;
    public string ApartmentCode { get; set; } = string.Empty;
    public string ResidentName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }

    [Range(0.01, 100000000, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [DataType(DataType.Date)]
    public DateTime PaymentDate { get; set; }

    [Required(ErrorMessage = "Payment method is required.")]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public List<string> PaymentMethodOptions { get; set; } = new();
}

public class ApartmentOptionViewModel
{
    public int ApartmentId { get; set; }
    public string ApartmentCode { get; set; } = string.Empty;
}
