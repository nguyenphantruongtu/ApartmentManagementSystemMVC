using BusinessObjects.Models;
using FinalProject_ApartmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ApartmentManagementSystem.Controllers;

[Authorize(Policy = "StaffOrAdmin")]
public class FeeTypesController : Controller
{
    private readonly AMSDbContext _dbContext;

    public FeeTypesController(AMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var feeTypes = await _dbContext.FeeTypes.AsNoTracking()
            .OrderBy(f => f.FeeName)
            .ToListAsync();

        var model = new FeeTypesIndexViewModel
        {
            FeeTypes = feeTypes.Select(f => new FeeTypeListItemViewModel
            {
                Id = f.Id,
                FeeCode = f.FeeCode,
                FeeName = f.FeeName,
                Description = f.Description,
                Unit = f.Unit,
                UnitPrice = f.UnitPrice,
                IsActive = f.IsActive,
                CreatedAt = f.CreatedAt
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new FeeTypeFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FeeTypeFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var feeCode = model.FeeCode.Trim();
        var feeName = model.FeeName.Trim();

        var existingCode = await _dbContext.FeeTypes
            .AnyAsync(f => f.FeeCode == feeCode);
        if (existingCode)
        {
            ModelState.AddModelError(nameof(model.FeeCode), "Fee code already exists.");
            return View(model);
        }

        var feeType = new FeeType
        {
            FeeCode = feeCode,
            FeeName = feeName,
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            Unit = string.IsNullOrWhiteSpace(model.Unit) ? null : model.Unit.Trim(),
            UnitPrice = model.UnitPrice,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.FeeTypes.Add(feeType);
        await _dbContext.SaveChangesAsync();

        TempData["FeeTypeSuccess"] = "Da tao loai phi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var feeType = await _dbContext.FeeTypes.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id);
        if (feeType is null)
        {
            return NotFound();
        }

        var model = new FeeTypeFormViewModel
        {
            Id = feeType.Id,
            FeeCode = feeType.FeeCode,
            FeeName = feeType.FeeName,
            Description = feeType.Description,
            Unit = feeType.Unit,
            UnitPrice = feeType.UnitPrice,
            IsActive = feeType.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FeeTypeFormViewModel model)
    {
        if (model.Id.HasValue && model.Id.Value != id)
        {
            ModelState.AddModelError(string.Empty, "Invalid fee type selection.");
        }

        var feeType = await _dbContext.FeeTypes.FirstOrDefaultAsync(f => f.Id == id);
        if (feeType is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var feeCode = model.FeeCode.Trim();
        var feeName = model.FeeName.Trim();
        var existingCode = await _dbContext.FeeTypes
            .AnyAsync(f => f.FeeCode == feeCode && f.Id != id);
        if (existingCode)
        {
            ModelState.AddModelError(nameof(model.FeeCode), "Fee code already exists.");
            return View(model);
        }

        feeType.FeeCode = feeCode;
        feeType.FeeName = feeName;
        feeType.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        feeType.Unit = string.IsNullOrWhiteSpace(model.Unit) ? null : model.Unit.Trim();
        feeType.UnitPrice = model.UnitPrice;
        feeType.IsActive = model.IsActive;

        await _dbContext.SaveChangesAsync();
        TempData["FeeTypeSuccess"] = "Da cap nhat loai phi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var feeType = await _dbContext.FeeTypes.FirstOrDefaultAsync(f => f.Id == id);
        if (feeType is null)
        {
            return NotFound();
        }

        feeType.IsActive = !feeType.IsActive;
        await _dbContext.SaveChangesAsync();

        TempData["FeeTypeSuccess"] = feeType.IsActive
            ? "Da kich hoat loai phi."
            : "Da tam khoa loai phi.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var feeType = await _dbContext.FeeTypes
            .Include(f => f.InvoiceDetails)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (feeType is null)
        {
            return NotFound();
        }

        if (feeType.InvoiceDetails.Count > 0)
        {
            TempData["FeeTypeError"] = "Khong the xoa loai phi da duoc su dung.";
            return RedirectToAction(nameof(Index));
        }

        _dbContext.FeeTypes.Remove(feeType);
        await _dbContext.SaveChangesAsync();
        TempData["FeeTypeSuccess"] = "Da xoa loai phi.";
        return RedirectToAction(nameof(Index));
    }
}
