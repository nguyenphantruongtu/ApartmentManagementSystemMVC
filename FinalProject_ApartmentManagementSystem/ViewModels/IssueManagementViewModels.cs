using System.ComponentModel.DataAnnotations;

namespace FinalProject_ApartmentManagementSystem.ViewModels;

public class IssueCategoriesIndexViewModel
{
    public List<IssueCategoryListItemViewModel> Categories { get; set; } = new();
}

public class IssueCategoryListItemViewModel
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PriorityLevel { get; set; }
    public int EstimatedResolutionDays { get; set; }
    public bool IsActive { get; set; }
    public int IssueCount { get; set; }
}

public class IssueCategoryFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [MaxLength(255)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(1, 5, ErrorMessage = "Priority level must be between 1 and 5.")]
    public int PriorityLevel { get; set; } = 3;

    [Range(1, 365, ErrorMessage = "Estimated resolution days must be between 1 and 365.")]
    public int EstimatedResolutionDays { get; set; } = 3;

    public bool IsActive { get; set; } = true;
}

public class IssuesIndexViewModel
{
    public List<IssueListItemViewModel> Issues { get; set; } = new();
    public string? StatusFilter { get; set; }
    public int? CategoryFilter { get; set; }
    public int? PriorityFilter { get; set; }
    public string? Search { get; set; }
    public List<string> StatusOptions { get; set; } = new();
    public List<IssueCategoryOptionViewModel> CategoryOptions { get; set; } = new();
    public bool CanManage { get; set; }
    public bool CanCreate { get; set; }
}

public class IssueListItemViewModel
{
    public int Id { get; set; }
    public string IssueCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string ApartmentCode { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ReportedByName { get; set; } = string.Empty;
    public string? AssignedToName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class IssueCreateViewModel
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public int CategoryId { get; set; }

    [MaxLength(2000)]
    public string? ImageUrls { get; set; }

    public List<IssueCategoryOptionViewModel> CategoryOptions { get; set; } = new();
}

public class IssueProcessViewModel
{
    public int Id { get; set; }
    public string IssueCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status is required.")]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    public int? AssignedToUserId { get; set; }

    [MaxLength(2000)]
    public string? ResolutionNotes { get; set; }

    public List<string> StatusOptions { get; set; } = new();
    public List<IssueAssigneeOptionViewModel> AssigneeOptions { get; set; } = new();
}

public class IssueDetailsViewModel
{
    public int Id { get; set; }
    public string IssueCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ApartmentCode { get; set; } = string.Empty;
    public string? BuildingName { get; set; }
    public int Priority { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ReportedByName { get; set; } = string.Empty;
    public string? AssignedToName { get; set; }
    public string? ImageUrls { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ResolvedDate { get; set; }
}

public class IssueCategoryOptionViewModel
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int PriorityLevel { get; set; }
}

public class IssueAssigneeOptionViewModel
{
    public int UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? RoleLabel { get; set; }
}
