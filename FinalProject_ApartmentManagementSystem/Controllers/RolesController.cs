using FinalProject_ApartmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace FinalProject_ApartmentManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class RolesController : Controller
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roles = await _roleService.GetRolesAsync();
        var model = new RolesIndexViewModel
        {
            Roles = roles.Select(r => new RoleListItemViewModel
            {
                Id = r.Id,
                RoleName = r.RoleName,
                Description = r.Description,
                UserCount = r.UserRoles.Count
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new RoleFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _roleService.CreateRoleAsync(model.RoleName, model.Description);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to create role.");
            return View(model);
        }

        TempData["RoleSuccess"] = "Đã tạo vai trò thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var role = await _roleService.GetRoleAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        return View(new RoleFormViewModel
        {
            Id = role.Id,
            RoleName = role.RoleName,
            Description = role.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RoleFormViewModel model)
    {
        if (model.Id.HasValue && model.Id.Value != id)
        {
            ModelState.AddModelError(string.Empty, "Invalid role selection.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _roleService.UpdateRoleAsync(id, model.RoleName, model.Description);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to update role.");
            return View(model);
        }

        TempData["RoleSuccess"] = "Đã cập nhật vai trò thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _roleService.DeleteRoleAsync(id);
        TempData["RoleSuccess"] = result.Succeeded
            ? "Đã xóa vai trò."
            : result.ErrorMessage ?? "Không thể xóa vai trò.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var users = await _roleService.GetUsersWithRolesAsync();
        var model = new UsersIndexViewModel
        {
            Users = users.Select(u => new UserRoleListItemViewModel
            {
                UserId = u.Id,
                FullName = u.FullName,
                Username = u.Username,
                Email = u.Email,
                Roles = u.UserRoles.Select(ur => ur.Role.RoleName).OrderBy(r => r).ToArray()
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditUserRoles(int id)
    {
        var user = await _roleService.GetUserWithRolesAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await _roleService.GetRolesAsync();
        var selectedIds = user.UserRoles.Select(ur => ur.RoleId).ToHashSet();

        var model = new RoleAssignmentViewModel
        {
            UserId = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            Email = user.Email,
            SelectedRoleIds = selectedIds.ToList(),
            RoleOptions = roles.Select(r => new RoleOptionViewModel
            {
                RoleId = r.Id,
                RoleName = r.RoleName
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUserRoles(int id, RoleAssignmentViewModel model)
    {
        if (id != model.UserId)
        {
            ModelState.AddModelError(string.Empty, "Invalid user selection.");
        }

        var selectedIds = model.SelectedRoleIds ?? new List<int>();
        var result = await _roleService.UpdateUserRolesAsync(id, selectedIds);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to update roles.");
        }

        if (!ModelState.IsValid)
        {
            var roles = await _roleService.GetRolesAsync();
            model.RoleOptions = roles.Select(r => new RoleOptionViewModel
            {
                RoleId = r.Id,
                RoleName = r.RoleName
            }).ToList();
            return View(model);
        }

        TempData["RoleSuccess"] = "Đã cập nhật vai trò cho người dùng.";
        return RedirectToAction(nameof(Users));
    }
}
