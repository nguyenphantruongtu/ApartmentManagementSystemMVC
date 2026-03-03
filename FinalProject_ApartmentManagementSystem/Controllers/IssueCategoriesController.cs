using FinalProject_ApartmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories;

namespace FinalProject_ApartmentManagementSystem.Controllers;

[Authorize(Policy = "AdminOnly")]
public class IssueCategoriesController : Controller
{
    private readonly IIssueCategoryRepository _issueCategoryRepository;

    public IssueCategoriesController(IIssueCategoryRepository issueCategoryRepository)
    {
        _issueCategoryRepository = issueCategoryRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categories = await _issueCategoryRepository.GetCategoriesAsync();
        var model = new IssueCategoriesIndexViewModel
        {
            Categories = categories.Select(c => new IssueCategoryListItemViewModel
            {
                Id = c.Id,
                CategoryName = c.CategoryName,
                Description = c.Description,
                PriorityLevel = c.PriorityLevel,
                EstimatedResolutionDays = c.EstimatedResolutionDays,
                IsActive = c.IsActive,
                IssueCount = c.Issues.Count
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new IssueCategoryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IssueCategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var category = new BusinessObjects.Models.IssueCategory
        {
            CategoryName = model.CategoryName.Trim(),
            Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
            PriorityLevel = model.PriorityLevel,
            EstimatedResolutionDays = model.EstimatedResolutionDays,
            IsActive = model.IsActive
        };

        await _issueCategoryRepository.CreateCategoryAsync(category);
        TempData["IssueCategorySuccess"] = "Da tao danh muc su co.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _issueCategoryRepository.GetCategoryByIdAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        var model = new IssueCategoryFormViewModel
        {
            Id = category.Id,
            CategoryName = category.CategoryName,
            Description = category.Description,
            PriorityLevel = category.PriorityLevel,
            EstimatedResolutionDays = category.EstimatedResolutionDays,
            IsActive = category.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, IssueCategoryFormViewModel model)
    {
        if (model.Id.HasValue && model.Id.Value != id)
        {
            ModelState.AddModelError(string.Empty, "Invalid category selection.");
        }

        var category = await _issueCategoryRepository.GetCategoryByIdAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        category.CategoryName = model.CategoryName.Trim();
        category.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        category.PriorityLevel = model.PriorityLevel;
        category.EstimatedResolutionDays = model.EstimatedResolutionDays;
        category.IsActive = model.IsActive;

        await _issueCategoryRepository.UpdateCategoryAsync(category);
        TempData["IssueCategorySuccess"] = "Da cap nhat danh muc su co.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var category = await _issueCategoryRepository.GetCategoryByIdAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        category.IsActive = !category.IsActive;
        await _issueCategoryRepository.UpdateCategoryAsync(category);

        TempData["IssueCategorySuccess"] = category.IsActive
            ? "Da kich hoat danh muc."
            : "Da tam khoa danh muc.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _issueCategoryRepository.GetCategoryByIdAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        if (category.Issues.Count > 0)
        {
            TempData["IssueCategoryError"] = "Khong the xoa danh muc da co su co.";
            return RedirectToAction(nameof(Index));
        }

        await _issueCategoryRepository.DeleteCategoryAsync(category);
        TempData["IssueCategorySuccess"] = "Da xoa danh muc su co.";
        return RedirectToAction(nameof(Index));
    }
}
