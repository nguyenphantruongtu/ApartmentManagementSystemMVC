using System.Security.Claims;
using BusinessObjects.Models;
using FinalProject_ApartmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ApartmentManagementSystem.Controllers;

[Authorize(Policy = "AnyRole")]
public class InvoicesController : Controller
{
    private static readonly List<string> StatusOptions = new()
    {
        "Unpaid",
        "PartiallyPaid",
        "Paid",
        "Overdue"
    };

    private readonly AMSDbContext _dbContext;

    public InvoicesController(AMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? status, int? month, int? year, int? apartmentId)
    {
        var isResident = User.IsInRole("Resident");
        var userId = GetCurrentUserId();
        var residentId = await GetResidentIdAsync(userId);

        var query = _dbContext.Invoices.AsNoTracking()
            .Include(i => i.Apartment)
            .Include(i => i.Resident)
            .AsQueryable();

        if (isResident)
        {
            if (!residentId.HasValue)
            {
                return View(new InvoiceIndexViewModel());
            }

            query = query.Where(i => i.ResidentId == residentId.Value);
        }

        if (month.HasValue)
        {
            query = query.Where(i => i.BillingMonth == month.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(i => i.BillingYear == year.Value);
        }

        if (apartmentId.HasValue)
        {
            query = query.Where(i => i.ApartmentId == apartmentId.Value);
        }

        var invoices = await query
            .OrderByDescending(i => i.BillingYear)
            .ThenByDescending(i => i.BillingMonth)
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var list = invoices.Select(i => new InvoiceListItemViewModel
        {
            Id = i.Id,
            InvoiceCode = i.InvoiceCode,
            ApartmentCode = i.Apartment.ApartmentCode,
            ResidentName = i.Resident.FullName,
            BillingMonth = i.BillingMonth,
            BillingYear = i.BillingYear,
            IssueDate = i.IssueDate,
            DueDate = i.DueDate,
            TotalAmount = i.TotalAmount,
            PaidAmount = i.PaidAmount,
            Status = CalculateInvoiceStatus(i, today)
        }).ToList();

        if (!string.IsNullOrWhiteSpace(status))
        {
            list = list.Where(i => string.Equals(i.Status, status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var model = new InvoiceIndexViewModel
        {
            Filter = new InvoiceFilterViewModel
            {
                Status = status,
                Month = month,
                Year = year,
                ApartmentId = apartmentId,
                StatusOptions = new List<string>(StatusOptions)
            },
            Invoices = list
        };

        if (!isResident)
        {
            var apartments = await _dbContext.Apartments.AsNoTracking()
                .OrderBy(a => a.ApartmentCode)
                .Select(a => new ApartmentOptionViewModel
                {
                    ApartmentId = a.Id,
                    ApartmentCode = a.ApartmentCode
                }).ToListAsync();

            model.ApartmentOptions = apartments;
        }

        return View(model);
    }

    [HttpGet]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<IActionResult> Create()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var model = new InvoiceCreateViewModel
        {
            BillingMonth = today.Month,
            BillingYear = today.Year,
            IssueDate = today,
            DueDate = today.AddDays(15)
        };

        await PopulateInvoiceFormOptionsAsync(model);
        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = "StaffOrAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InvoiceCreateViewModel model)
    {
        if (model.DueDate < model.IssueDate)
        {
            ModelState.AddModelError(nameof(model.DueDate), "Due date must be after issue date.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateInvoiceFormOptionsAsync(model);
            return View(model);
        }

        var apartment = await _dbContext.Apartments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == model.ApartmentId);
        if (apartment is null)
        {
            ModelState.AddModelError(nameof(model.ApartmentId), "Apartment not found.");
        }

        var resident = await _dbContext.Residents.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == model.ResidentId);
        if (resident is null)
        {
            ModelState.AddModelError(nameof(model.ResidentId), "Resident not found.");
        }

        var hasStay = await _dbContext.ApartmentResidents.AsNoTracking()
            .AnyAsync(ar => ar.ApartmentId == model.ApartmentId && ar.ResidentId == model.ResidentId && ar.IsActive);
        if (!hasStay)
        {
            ModelState.AddModelError(nameof(model.ResidentId), "Resident is not active in the selected apartment.");
        }

        var duplicate = await _dbContext.Invoices.AsNoTracking()
            .AnyAsync(i => i.ApartmentId == model.ApartmentId
                           && i.BillingMonth == model.BillingMonth
                           && i.BillingYear == model.BillingYear);
        if (duplicate)
        {
            ModelState.AddModelError(nameof(model.BillingMonth), "Invoice already exists for this apartment and period.");
        }

        var feeTypes = await _dbContext.FeeTypes.AsNoTracking()
            .Where(f => f.IsActive)
            .ToListAsync();
        var feeTypeMap = feeTypes.ToDictionary(f => f.Id, f => f);

        var selectedItems = model.FeeItems
            .Where(i => i.Quantity > 0)
            .ToList();
        if (selectedItems.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Please select at least one fee item.");
        }

        foreach (var item in selectedItems)
        {
            if (!feeTypeMap.ContainsKey(item.FeeTypeId))
            {
                ModelState.AddModelError(string.Empty, "Invalid fee type selection.");
                break;
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateInvoiceFormOptionsAsync(model);
            return View(model);
        }

        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Forbid();
        }

        var details = selectedItems.Select(item =>
        {
            var fee = feeTypeMap[item.FeeTypeId];
            var amount = fee.UnitPrice * item.Quantity;
            return new InvoiceDetail
            {
                FeeTypeId = fee.Id,
                Quantity = item.Quantity,
                UnitPrice = fee.UnitPrice,
                Amount = amount,
                Description = fee.FeeName
            };
        }).ToList();

        var totalAmount = details.Sum(d => d.Amount);
        var invoiceCode = await GenerateInvoiceCodeAsync(model.BillingYear, model.BillingMonth, apartment!.ApartmentCode);

        var invoice = new Invoice
        {
            InvoiceCode = invoiceCode,
            ApartmentId = model.ApartmentId,
            ResidentId = model.ResidentId,
            BillingMonth = model.BillingMonth,
            BillingYear = model.BillingYear,
            IssueDate = model.IssueDate,
            DueDate = model.DueDate,
            TotalAmount = totalAmount,
            PaidAmount = 0,
            Status = "Unpaid",
            Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(),
            CreatedBy = userId.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            InvoiceDetails = details
        };

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        TempData["InvoiceSuccess"] = "Da tao hoa don.";
        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var invoice = await _dbContext.Invoices
            .Include(i => i.Apartment)
            .Include(i => i.Resident)
            .Include(i => i.InvoiceDetails)
                .ThenInclude(d => d.FeeType)
            .Include(i => i.Payments)
                .ThenInclude(p => p.ReceivedByNavigation)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null)
        {
            return NotFound();
        }

        var isResident = User.IsInRole("Resident");
        if (isResident)
        {
            var userId = GetCurrentUserId();
            var residentId = await GetResidentIdAsync(userId);
            if (!residentId.HasValue || invoice.ResidentId != residentId.Value)
            {
                return Forbid();
            }
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var newStatus = CalculateInvoiceStatus(invoice, today);
        if (!string.Equals(invoice.Status, newStatus, StringComparison.OrdinalIgnoreCase))
        {
            invoice.Status = newStatus;
            invoice.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        var model = new InvoiceDetailsViewModel
        {
            Id = invoice.Id,
            InvoiceCode = invoice.InvoiceCode,
            ApartmentCode = invoice.Apartment.ApartmentCode,
            ResidentName = invoice.Resident.FullName,
            BillingMonth = invoice.BillingMonth,
            BillingYear = invoice.BillingYear,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            Status = invoice.Status,
            Notes = invoice.Notes,
            Details = invoice.InvoiceDetails.Select(d => new InvoiceDetailLineViewModel
            {
                FeeName = d.FeeType.FeeName,
                Unit = d.FeeType.Unit,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Amount = d.Amount
            }).ToList(),
            Payments = invoice.Payments
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentListItemViewModel
                {
                    Id = p.Id,
                    InvoiceCode = invoice.InvoiceCode,
                    ApartmentCode = invoice.Apartment.ApartmentCode,
                    ResidentName = invoice.Resident.FullName,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod,
                    ReferenceNumber = p.ReferenceNumber,
                    ReceivedByName = p.ReceivedByNavigation?.FullName
                }).ToList()
        };

        return View(model);
    }

    private async Task PopulateInvoiceFormOptionsAsync(InvoiceCreateViewModel model)
    {
        var apartments = await _dbContext.Apartments.AsNoTracking()
            .OrderBy(a => a.ApartmentCode)
            .Select(a => new ApartmentOptionViewModel
            {
                ApartmentId = a.Id,
                ApartmentCode = a.ApartmentCode
            }).ToListAsync();

        var residents = await _dbContext.Residents.AsNoTracking()
            .OrderBy(r => r.FullName)
            .Select(r => new ResidentOptionViewModel
            {
                ResidentId = r.Id,
                FullName = r.FullName,
                Phone = r.Phone,
                Email = r.Email
            }).ToListAsync();

        var feeTypes = await _dbContext.FeeTypes.AsNoTracking()
            .Where(f => f.IsActive)
            .OrderBy(f => f.FeeName)
            .ToListAsync();

        var quantityLookup = model.FeeItems.ToDictionary(i => i.FeeTypeId, i => i.Quantity);

        model.ApartmentOptions = apartments;
        model.ResidentOptions = residents;
        model.FeeItems = feeTypes.Select(f => new FeeTypeQuantityViewModel
        {
            FeeTypeId = f.Id,
            FeeName = f.FeeName,
            Unit = f.Unit,
            UnitPrice = f.UnitPrice,
            Quantity = quantityLookup.TryGetValue(f.Id, out var qty) ? qty : 0
        }).ToList();
    }

    private async Task<string> GenerateInvoiceCodeAsync(int year, int month, string apartmentCode)
    {
        var baseCode = $"INV-{year}{month:D2}-{apartmentCode}";
        var code = baseCode;
        var suffix = 1;

        while (await _dbContext.Invoices.AsNoTracking().AnyAsync(i => i.InvoiceCode == code))
        {
            code = $"{baseCode}-{suffix}";
            suffix++;
        }

        return code;
    }

    private static string CalculateInvoiceStatus(Invoice invoice, DateOnly today)
    {
        if (invoice.TotalAmount <= 0)
        {
            return "Unpaid";
        }

        if (invoice.PaidAmount >= invoice.TotalAmount)
        {
            return "Paid";
        }

        if (invoice.DueDate < today)
        {
            return "Overdue";
        }

        if (invoice.PaidAmount > 0)
        {
            return "PartiallyPaid";
        }

        return "Unpaid";
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private async Task<int?> GetResidentIdAsync(int? userId)
    {
        if (!userId.HasValue)
        {
            return null;
        }

        var resident = await _dbContext.Residents.AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId.Value);
        return resident?.Id;
    }
}
