using System.Security.Claims;
using BusinessObjects.Models;
using FinalProject_ApartmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace FinalProject_ApartmentManagementSystem.Controllers;

[Authorize(Policy = "AnyRole")]
public class IssuesController : Controller
{
    private static readonly List<string> StatusOptions = new()
    {
        "Open",
        "InProgress",
        "Resolved",
        "Closed"
    };

    private readonly IIssueRepository _issueRepository;
    private readonly IIssueCategoryRepository _issueCategoryRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly AMSDbContext _dbContext;

    public IssuesController(
        IIssueRepository issueRepository,
        IIssueCategoryRepository issueCategoryRepository,
        IRoleRepository roleRepository,
        AMSDbContext dbContext)
    {
        _issueRepository = issueRepository;
        _issueCategoryRepository = issueCategoryRepository;
        _roleRepository = roleRepository;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? status, int? categoryId, int? priority, string? search)
    {
        var userId = GetCurrentUserId();
        var canManage = User.IsInRole("Admin") || User.IsInRole("Staff");
        var canCreate = User.IsInRole("Resident");

        var issues = canManage
            ? await _issueRepository.GetIssuesAsync()
            : userId.HasValue
                ? await _issueRepository.GetIssuesByReporterAsync(userId.Value)
                : new List<Issue>();

        var filtered = issues.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            filtered = filtered.Where(i => string.Equals(i.Status, status, StringComparison.OrdinalIgnoreCase));
        }

        if (categoryId.HasValue)
        {
            filtered = filtered.Where(i => i.CategoryId == categoryId.Value);
        }

        if (priority.HasValue)
        {
            filtered = filtered.Where(i => i.Priority == priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            filtered = filtered.Where(i =>
                i.IssueCode.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                i.Title.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var categories = await _issueCategoryRepository.GetCategoriesAsync();
        var model = new IssuesIndexViewModel
        {
            StatusFilter = status,
            CategoryFilter = categoryId,
            PriorityFilter = priority,
            Search = search,
            StatusOptions = StatusOptions,
            CategoryOptions = categories.Select(c => new IssueCategoryOptionViewModel
            {
                CategoryId = c.Id,
                CategoryName = c.CategoryName,
                PriorityLevel = c.PriorityLevel
            }).ToList(),
            CanManage = canManage,
            CanCreate = canCreate,
            Issues = filtered.Select(i => new IssueListItemViewModel
            {
                Id = i.Id,
                IssueCode = i.IssueCode,
                Title = i.Title,
                CategoryName = i.Category?.CategoryName ?? "-",
                ApartmentCode = i.Apartment?.ApartmentCode ?? "-",
                Priority = i.Priority,
                Status = i.Status,
                ReportedByName = i.ReportedByUser?.FullName ?? "-",
                AssignedToName = i.AssignedToUser?.FullName,
                CreatedAt = i.CreatedAt
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    [Authorize(Policy = "ResidentOnly")]
    public async Task<IActionResult> Create()
    {
        var model = new IssueCreateViewModel
        {
            CategoryOptions = await GetActiveCategoryOptionsAsync()
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = "ResidentOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IssueCreateViewModel model)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Forbid();
        }

        var categories = await GetActiveCategoryOptionsAsync();
        if (!ModelState.IsValid)
        {
            model.CategoryOptions = categories;
            return View(model);
        }

        var category = await _issueCategoryRepository.GetCategoryByIdAsync(model.CategoryId);
        if (category is null || !category.IsActive)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Danh muc khong hop le.");
            model.CategoryOptions = categories;
            return View(model);
        }

        var resident = await _dbContext.Residents.AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId.Value);
        if (resident is null)
        {
            ModelState.AddModelError(string.Empty, "Khong tim thay thong tin cu dan.");
            model.CategoryOptions = categories;
            return View(model);
        }

        var activeStay = await _dbContext.ApartmentResidents.AsNoTracking()
            .Where(ar => ar.ResidentId == resident.Id && ar.IsActive)
            .OrderByDescending(ar => ar.MoveInDate)
            .FirstOrDefaultAsync();

        if (activeStay is null)
        {
            ModelState.AddModelError(string.Empty, "Ban chua duoc gan can ho hoat dong.");
            model.CategoryOptions = categories;
            return View(model);
        }

        var now = DateTime.UtcNow;
        var issue = new Issue
        {
            IssueCode = await GenerateIssueCodeAsync(),
            Title = model.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            CategoryId = category.Id,
            ApartmentId = activeStay.ApartmentId,
            ReportedByUserId = userId.Value,
            Priority = category.PriorityLevel,
            Status = "Open",
            ImageUrls = string.IsNullOrWhiteSpace(model.ImageUrls) ? null : model.ImageUrls.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _issueRepository.CreateIssueAsync(issue);
        TempData["IssueSuccess"] = "Da ghi nhan bao cao su co.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var issue = await _issueRepository.GetIssueByIdAsync(id);
        if (issue is null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        var canManage = User.IsInRole("Admin") || User.IsInRole("Staff");
        if (!canManage && (!userId.HasValue || issue.ReportedByUserId != userId.Value))
        {
            return Forbid();
        }

        var model = new IssueDetailsViewModel
        {
            Id = issue.Id,
            IssueCode = issue.IssueCode,
            Title = issue.Title,
            Description = issue.Description,
            CategoryName = issue.Category?.CategoryName ?? "-",
            ApartmentCode = issue.Apartment?.ApartmentCode ?? "-",
            BuildingName = issue.Apartment?.Building?.BuildingName,
            Priority = issue.Priority,
            Status = issue.Status,
            ReportedByName = issue.ReportedByUser?.FullName ?? "-",
            AssignedToName = issue.AssignedToUser?.FullName,
            ImageUrls = issue.ImageUrls,
            ResolutionNotes = issue.ResolutionNotes,
            CreatedAt = issue.CreatedAt,
            UpdatedAt = issue.UpdatedAt,
            ResolvedDate = issue.ResolvedDate
        };

        return View(model);
    }

    [HttpGet]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<IActionResult> Edit(int id)
    {
        var issue = await _issueRepository.GetIssueByIdAsync(id);
        if (issue is null)
        {
            return NotFound();
        }

        var model = new IssueProcessViewModel
        {
            Id = issue.Id,
            IssueCode = issue.IssueCode,
            Title = issue.Title,
            Status = issue.Status,
            AssignedToUserId = issue.AssignedToUserId,
            ResolutionNotes = issue.ResolutionNotes,
            StatusOptions = StatusOptions,
            AssigneeOptions = await GetAssigneeOptionsAsync()
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = "StaffOrAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, IssueProcessViewModel model)
    {
        if (model.Id != id)
        {
            ModelState.AddModelError(string.Empty, "Invalid issue selection.");
        }

        var issue = await _issueRepository.GetIssueByIdAsync(id);
        if (issue is null)
        {
            return NotFound();
        }

        if (!StatusOptions.Contains(model.Status))
        {
            ModelState.AddModelError(nameof(model.Status), "Trang thai khong hop le.");
        }

        var assignees = await GetAssigneeOptionsAsync();
        if (model.AssignedToUserId.HasValue &&
            assignees.All(a => a.UserId != model.AssignedToUserId.Value))
        {
            ModelState.AddModelError(nameof(model.AssignedToUserId), "Nguoi xu ly khong hop le.");
        }

        if (!ModelState.IsValid)
        {
            model.StatusOptions = StatusOptions;
            model.AssigneeOptions = assignees;
            return View(model);
        }

        issue.Status = model.Status;
        issue.AssignedToUserId = model.AssignedToUserId;
        issue.ResolutionNotes = string.IsNullOrWhiteSpace(model.ResolutionNotes)
            ? null
            : model.ResolutionNotes.Trim();

        if (issue.Status is "Resolved" or "Closed")
        {
            issue.ResolvedDate ??= DateTime.UtcNow;
        }
        else
        {
            issue.ResolvedDate = null;
        }

        issue.UpdatedAt = DateTime.UtcNow;

        await _issueRepository.UpdateIssueAsync(issue);
        TempData["IssueSuccess"] = "Da cap nhat xu ly su co.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<IssueCategoryOptionViewModel>> GetActiveCategoryOptionsAsync()
    {
        var categories = await _issueCategoryRepository.GetActiveCategoriesAsync();
        return categories.Select(c => new IssueCategoryOptionViewModel
        {
            CategoryId = c.Id,
            CategoryName = c.CategoryName,
            PriorityLevel = c.PriorityLevel
        }).ToList();
    }

    private async Task<List<IssueAssigneeOptionViewModel>> GetAssigneeOptionsAsync()
    {
        var users = await _roleRepository.GetUsersWithRolesAsync();
        return users
            .Where(u => u.UserRoles.Any(ur =>
                ur.Role is not null &&
                (ur.Role.RoleName == "Admin" || ur.Role.RoleName == "Staff")))
            .OrderBy(u => u.FullName)
            .Select(u => new IssueAssigneeOptionViewModel
            {
                UserId = u.Id,
                DisplayName = u.FullName,
                RoleLabel = string.Join(", ", u.UserRoles
                    .Select(ur => ur.Role?.RoleName)
                    .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                    .Distinct(StringComparer.OrdinalIgnoreCase))
            }).ToList();
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private async Task<string> GenerateIssueCodeAsync()
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var code = $"ISS-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
            var exists = await _dbContext.Issues.AsNoTracking().AnyAsync(i => i.IssueCode == code);
            if (!exists)
            {
                return code;
            }
        }

        return $"ISS-{Guid.NewGuid():N}".Substring(0, 18).ToUpperInvariant();
    }
}
