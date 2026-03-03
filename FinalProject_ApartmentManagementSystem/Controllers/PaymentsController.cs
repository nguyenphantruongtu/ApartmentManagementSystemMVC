using System.Security.Claims;
using BusinessObjects.Models;
using FinalProject_ApartmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ApartmentManagementSystem.Controllers;

[Authorize(Policy = "AnyRole")]
public class PaymentsController : Controller
{
    private static readonly List<string> DefaultPaymentMethods = new()
    {
        "Cash",
        "BankTransfer",
        "EWallet"
    };

    private readonly AMSDbContext _dbContext;

    public PaymentsController(AMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var isResident = User.IsInRole("Resident");
        var userId = GetCurrentUserId();
        var residentId = await GetResidentIdAsync(userId);

        var query = _dbContext.Payments.AsNoTracking()
            .Include(p => p.Invoice)
                .ThenInclude(i => i.Apartment)
            .Include(p => p.Invoice)
                .ThenInclude(i => i.Resident)
            .Include(p => p.ReceivedByNavigation)
            .AsQueryable();

        if (isResident)
        {
            if (!residentId.HasValue)
            {
                return View(new PaymentIndexViewModel());
            }

            query = query.Where(p => p.Invoice.ResidentId == residentId.Value);
        }

        var payments = await query
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        var model = new PaymentIndexViewModel
        {
            Payments = payments.Select(p => new PaymentListItemViewModel
            {
                Id = p.Id,
                InvoiceCode = p.Invoice.InvoiceCode,
                ApartmentCode = p.Invoice.Apartment.ApartmentCode,
                ResidentName = p.Invoice.Resident.FullName,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                ReferenceNumber = p.ReferenceNumber,
                ReceivedByName = p.ReceivedByNavigation?.FullName
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int invoiceId)
    {
        var invoice = await _dbContext.Invoices.AsNoTracking()
            .Include(i => i.Apartment)
            .Include(i => i.Resident)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice is null)
        {
            return NotFound();
        }

        if (!await CanAccessInvoiceAsync(invoice))
        {
            return Forbid();
        }

        var model = new PaymentCreateViewModel
        {
            InvoiceId = invoice.Id,
            InvoiceCode = invoice.InvoiceCode,
            ApartmentCode = invoice.Apartment.ApartmentCode,
            ResidentName = invoice.Resident.FullName,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            Amount = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount),
            PaymentDate = DateTime.Today,
            PaymentMethodOptions = new List<string>(DefaultPaymentMethods)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentCreateViewModel model)
    {
        var invoice = await _dbContext.Invoices
            .Include(i => i.Apartment)
            .Include(i => i.Resident)
            .FirstOrDefaultAsync(i => i.Id == model.InvoiceId);

        if (invoice is null)
        {
            return NotFound();
        }

        if (!await CanAccessInvoiceAsync(invoice))
        {
            return Forbid();
        }

        var remaining = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);
        if (model.Amount > remaining)
        {
            ModelState.AddModelError(nameof(model.Amount), "Amount exceeds remaining balance.");
        }

        if (!ModelState.IsValid)
        {
            model.InvoiceCode = invoice.InvoiceCode;
            model.ApartmentCode = invoice.Apartment.ApartmentCode;
            model.ResidentName = invoice.Resident.FullName;
            model.TotalAmount = invoice.TotalAmount;
            model.PaidAmount = invoice.PaidAmount;
            model.PaymentMethodOptions = new List<string>(DefaultPaymentMethods);
            return View(model);
        }

        var userId = GetCurrentUserId();
        var receivedBy = User.IsInRole("Admin") || User.IsInRole("Staff")
            ? userId
            : null;

        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            Amount = model.Amount,
            PaymentDate = model.PaymentDate,
            PaymentMethod = model.PaymentMethod.Trim(),
            ReferenceNumber = string.IsNullOrWhiteSpace(model.ReferenceNumber) ? null : model.ReferenceNumber.Trim(),
            ReceivedBy = receivedBy,
            Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Payments.Add(payment);
        invoice.PaidAmount += model.Amount;
        invoice.Status = CalculateInvoiceStatus(invoice, DateOnly.FromDateTime(DateTime.Today));
        invoice.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        TempData["PaymentSuccess"] = "Da ghi nhan thanh toan.";
        return RedirectToAction("Details", "Invoices", new { id = invoice.Id });
    }

    private async Task<bool> CanAccessInvoiceAsync(Invoice invoice)
    {
        if (!User.IsInRole("Resident"))
        {
            return true;
        }

        var userId = GetCurrentUserId();
        var residentId = await GetResidentIdAsync(userId);
        return residentId.HasValue && invoice.ResidentId == residentId.Value;
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
