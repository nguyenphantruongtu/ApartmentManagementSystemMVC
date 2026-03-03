using FinalProject_ApartmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace FinalProject_ApartmentManagementSystem.Controllers;

[Authorize(Policy = "StaffOrAdmin")]
public class ApartmentsController : Controller
{
    private static readonly List<string> DefaultStatusOptions = new()
    {
        "Available",
        "Occupied",
        "Maintenance"
    };

    private readonly IApartmentService _apartmentService;

    public ApartmentsController(IApartmentService apartmentService)
    {
        _apartmentService = apartmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var apartments = await _apartmentService.GetApartmentsAsync();
        var model = new ApartmentsIndexViewModel
        {
            Apartments = apartments.Select(a => new ApartmentListItemViewModel
            {
                Id = a.Id,
                ApartmentCode = a.ApartmentCode,
                BuildingName = a.Building.BuildingName,
                Floor = a.Floor,
                Area = a.Area,
                NumberOfRooms = a.NumberOfRooms,
                Status = a.Status,
                ActiveResidentCount = a.ApartmentResidents.Count(ar => ar.IsActive)
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ApartmentFormViewModel
        {
            Status = "Available",
            StatusOptions = new List<string>(DefaultStatusOptions)
        };

        await PopulateApartmentFormOptionsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApartmentFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateApartmentFormOptionsAsync(model);
            return View(model);
        }

        var result = await _apartmentService.CreateApartmentAsync(new ApartmentFormRequest(
            model.ApartmentCode,
            model.BuildingId,
            model.Floor,
            model.Area,
            model.NumberOfRooms,
            model.ApartmentType,
            model.Status,
            model.Description));

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to create apartment.");
            await PopulateApartmentFormOptionsAsync(model);
            return View(model);
        }

        TempData["ApartmentSuccess"] = "Đã tạo căn hộ thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var apartment = await _apartmentService.GetApartmentAsync(id);
        if (apartment is null)
        {
            return NotFound();
        }

        var model = new ApartmentFormViewModel
        {
            Id = apartment.Id,
            ApartmentCode = apartment.ApartmentCode,
            BuildingId = apartment.BuildingId,
            Floor = apartment.Floor,
            Area = apartment.Area,
            NumberOfRooms = apartment.NumberOfRooms,
            ApartmentType = apartment.ApartmentType,
            Status = apartment.Status,
            Description = apartment.Description,
            StatusOptions = new List<string>(DefaultStatusOptions)
        };

        await PopulateApartmentFormOptionsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ApartmentFormViewModel model)
    {
        if (model.Id.HasValue && model.Id.Value != id)
        {
            ModelState.AddModelError(string.Empty, "Invalid apartment selection.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateApartmentFormOptionsAsync(model);
            return View(model);
        }

        var result = await _apartmentService.UpdateApartmentAsync(id, new ApartmentFormRequest(
            model.ApartmentCode,
            model.BuildingId,
            model.Floor,
            model.Area,
            model.NumberOfRooms,
            model.ApartmentType,
            model.Status,
            model.Description));

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to update apartment.");
            await PopulateApartmentFormOptionsAsync(model);
            return View(model);
        }

        TempData["ApartmentSuccess"] = "Đã cập nhật căn hộ thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _apartmentService.DeleteApartmentAsync(id);
        TempData["ApartmentSuccess"] = result.Succeeded
            ? "Đã xóa căn hộ."
            : result.ErrorMessage ?? "Không thể xóa căn hộ.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var apartment = await _apartmentService.GetApartmentDetailAsync(id);
        if (apartment is null)
        {
            return NotFound();
        }

        var model = new ApartmentDetailsViewModel
        {
            Id = apartment.Id,
            ApartmentCode = apartment.ApartmentCode,
            BuildingName = apartment.Building.BuildingName,
            Floor = apartment.Floor,
            Area = apartment.Area,
            NumberOfRooms = apartment.NumberOfRooms,
            ApartmentType = apartment.ApartmentType,
            Status = apartment.Status,
            Description = apartment.Description,
            Residents = apartment.ApartmentResidents
                .OrderByDescending(ar => ar.IsActive)
                .ThenByDescending(ar => ar.MoveInDate)
                .Select(ar => new ApartmentResidentViewModel
                {
                    ApartmentResidentId = ar.Id,
                    ResidentId = ar.ResidentId,
                    FullName = ar.Resident.FullName,
                    IsOwner = ar.IsOwner,
                    MoveInDate = ar.MoveInDate,
                    MoveOutDate = ar.MoveOutDate,
                    IsActive = ar.IsActive,
                    Notes = ar.Notes
                }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> AssignResident(int id)
    {
        var apartment = await _apartmentService.GetApartmentAsync(id);
        if (apartment is null)
        {
            return NotFound();
        }

        var model = new AssignResidentViewModel
        {
            ApartmentId = apartment.Id,
            ApartmentCode = apartment.ApartmentCode,
            BuildingName = apartment.Building.BuildingName,
            MoveInDate = DateOnly.FromDateTime(DateTime.Today)
        };

        await PopulateResidentOptionsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignResident(int id, AssignResidentViewModel model)
    {
        if (id != model.ApartmentId)
        {
            ModelState.AddModelError(string.Empty, "Invalid apartment selection.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateResidentOptionsAsync(model);
            return View(model);
        }

        var result = await _apartmentService.AssignResidentAsync(new AssignResidentRequest(
            model.ApartmentId,
            model.ResidentId,
            model.IsOwner,
            model.MoveInDate,
            model.MoveOutDate,
            model.Notes));

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to assign resident.");
            await PopulateResidentOptionsAsync(model);
            return View(model);
        }

        TempData["ApartmentSuccess"] = "Đã gán cư dân vào căn hộ.";
        return RedirectToAction(nameof(Details), new { id = model.ApartmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EndResident(int apartmentResidentId, int apartmentId, DateOnly? moveOutDate)
    {
        var result = await _apartmentService.EndResidentStayAsync(apartmentResidentId, moveOutDate);
        TempData["ApartmentSuccess"] = result.Succeeded
            ? "Đã kết thúc cư trú."
            : result.ErrorMessage ?? "Không thể kết thúc cư trú.";

        return RedirectToAction(nameof(Details), new { id = apartmentId });
    }

    private async Task PopulateApartmentFormOptionsAsync(ApartmentFormViewModel model)
    {
        var buildings = await _apartmentService.GetBuildingsAsync();
        model.BuildingOptions = buildings.Select(b => new BuildingOptionViewModel
        {
            BuildingId = b.Id,
            BuildingName = b.BuildingName
        }).ToList();

        model.StatusOptions = model.StatusOptions.Count == 0
            ? new List<string>(DefaultStatusOptions)
            : model.StatusOptions;
    }

    private async Task PopulateResidentOptionsAsync(AssignResidentViewModel model)
    {
        var residents = await _apartmentService.GetResidentsAsync();
        model.ResidentOptions = residents.Select(r => new ResidentOptionViewModel
        {
            ResidentId = r.Id,
            FullName = r.FullName,
            Phone = r.Phone,
            Email = r.Email
        }).ToList();

        if (string.IsNullOrWhiteSpace(model.ApartmentCode))
        {
            var apartment = await _apartmentService.GetApartmentAsync(model.ApartmentId);
            if (apartment is not null)
            {
                model.ApartmentCode = apartment.ApartmentCode;
                model.BuildingName = apartment.Building.BuildingName;
            }
        }
    }
}
