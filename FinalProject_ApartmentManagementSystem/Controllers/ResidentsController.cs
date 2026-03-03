using BusinessObjects.Models;
using FinalProject_ApartmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace FinalProject_ApartmentManagementSystem.Controllers;

[Authorize(Policy = "StaffOrAdmin")]
public class ResidentsController : Controller
{
    private readonly IResidentRepository _residentRepository;
    private readonly AMSDbContext _dbContext;

    public ResidentsController(IResidentRepository residentRepository, AMSDbContext dbContext)
    {
        _residentRepository = residentRepository;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var residents = await _residentRepository.GetResidentsAsync();
        var model = new ResidentsIndexViewModel
        {
            Residents = residents.Select(r => new ResidentListItemViewModel
            {
                Id = r.Id,
                FullName = r.FullName,
                DateOfBirth = r.DateOfBirth,
                Gender = r.Gender,
                CitizenId = r.CitizenId,
                Phone = r.Phone,
                Email = r.Email
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ResidentFormViewModel
        {
            UserOptions = await GetAvailableUserOptionsAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResidentFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.UserOptions = await GetAvailableUserOptionsAsync();
            return View(model);
        }

        var user = await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == model.UserId);
        if (user is null)
        {
            ModelState.AddModelError(nameof(model.UserId), "User not found.");
        }

        var existingForUser = await _residentRepository.GetResidentByUserIdAsync(model.UserId);
        if (existingForUser is not null)
        {
            ModelState.AddModelError(nameof(model.UserId), "User already has a resident profile.");
        }

        if (!string.IsNullOrWhiteSpace(model.CitizenId))
        {
            var existingCitizen = await _residentRepository.GetResidentByCitizenIdAsync(model.CitizenId);
            if (existingCitizen is not null)
            {
                ModelState.AddModelError(nameof(model.CitizenId), "Citizen ID already exists.");
            }
        }

        if (!ModelState.IsValid)
        {
            model.UserOptions = await GetAvailableUserOptionsAsync();
            return View(model);
        }

        var resident = new Resident
        {
            UserId = model.UserId,
            FullName = model.FullName.Trim(),
            DateOfBirth = model.DateOfBirth,
            Gender = string.IsNullOrWhiteSpace(model.Gender) ? null : model.Gender.Trim(),
            CitizenId = string.IsNullOrWhiteSpace(model.CitizenId) ? null : model.CitizenId.Trim(),
            Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            EmergencyContact = string.IsNullOrWhiteSpace(model.EmergencyContact) ? null : model.EmergencyContact.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _residentRepository.CreateResidentAsync(resident);
        TempData["ResidentSuccess"] = "Da tao cu dan thanh cong.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var resident = await _residentRepository.GetResidentDetailAsync(id);
        if (resident is null)
        {
            return NotFound();
        }

        var model = new ResidentFormViewModel
        {
            Id = resident.Id,
            FullName = resident.FullName,
            DateOfBirth = resident.DateOfBirth,
            Gender = resident.Gender,
            CitizenId = resident.CitizenId,
            Phone = resident.Phone,
            Email = resident.Email,
            EmergencyContact = resident.EmergencyContact
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ResidentFormViewModel model)
    {
        if (model.Id.HasValue && model.Id.Value != id)
        {
            ModelState.AddModelError(string.Empty, "Invalid resident selection.");
        }

        var resident = await _residentRepository.GetResidentByIdAsync(id);
        if (resident is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(model.CitizenId))
        {
            var existingCitizen = await _residentRepository.GetResidentByCitizenIdAsync(model.CitizenId);
            if (existingCitizen is not null && existingCitizen.Id != id)
            {
                ModelState.AddModelError(nameof(model.CitizenId), "Citizen ID already exists.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        resident.FullName = model.FullName.Trim();
        resident.DateOfBirth = model.DateOfBirth;
        resident.Gender = string.IsNullOrWhiteSpace(model.Gender) ? null : model.Gender.Trim();
        resident.CitizenId = string.IsNullOrWhiteSpace(model.CitizenId) ? null : model.CitizenId.Trim();
        resident.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
        resident.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        resident.EmergencyContact = string.IsNullOrWhiteSpace(model.EmergencyContact) ? null : model.EmergencyContact.Trim();
        resident.UpdatedAt = DateTime.UtcNow;

        await _residentRepository.UpdateResidentAsync(resident);
        TempData["ResidentSuccess"] = "Da cap nhat cu dan thanh cong.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var resident = await _residentRepository.GetResidentDetailAsync(id);
        if (resident is null)
        {
            return NotFound();
        }

        var model = new ResidentDetailsViewModel
        {
            Id = resident.Id,
            FullName = resident.FullName,
            DateOfBirth = resident.DateOfBirth,
            Gender = resident.Gender,
            CitizenId = resident.CitizenId,
            Phone = resident.Phone,
            Email = resident.Email,
            EmergencyContact = resident.EmergencyContact,
            Username = resident.User?.Username,
            UserEmail = resident.User?.Email,
            CreatedAt = resident.CreatedAt,
            UpdatedAt = resident.UpdatedAt
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var resident = await _residentRepository.GetResidentByIdAsync(id);
        if (resident is null)
        {
            return NotFound();
        }

        var hasApartmentStay = await _dbContext.ApartmentResidents
            .AnyAsync(ar => ar.ResidentId == id);
        if (hasApartmentStay)
        {
            TempData["ResidentError"] = "Khong the xoa cu dan da co lich su can ho.";
            return RedirectToAction(nameof(Index));
        }

        var hasInvoices = await _dbContext.Invoices.AnyAsync(i => i.ResidentId == id);
        if (hasInvoices)
        {
            TempData["ResidentError"] = "Khong the xoa cu dan da co hoa don.";
            return RedirectToAction(nameof(Index));
        }

        await _residentRepository.DeleteResidentAsync(resident);
        TempData["ResidentSuccess"] = "Da xoa cu dan.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<UserOptionViewModel>> GetAvailableUserOptionsAsync()
    {
        return await _dbContext.Users.AsNoTracking()
            .Where(u => !_dbContext.Residents.Any(r => r.UserId == u.Id))
            .OrderBy(u => u.FullName)
            .Select(u => new UserOptionViewModel
            {
                UserId = u.Id,
                DisplayName = u.FullName,
                Email = u.Email
            }).ToListAsync();
    }
}
