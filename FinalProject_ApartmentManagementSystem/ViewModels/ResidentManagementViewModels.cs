using System.ComponentModel.DataAnnotations;

namespace FinalProject_ApartmentManagementSystem.ViewModels;

public class ResidentsIndexViewModel
{
    public List<ResidentListItemViewModel> Residents { get; set; } = new();
}

public class ResidentListItemViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? CitizenId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class ResidentFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "User is required.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    [MaxLength(20)]
    public string? CitizenId { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? EmergencyContact { get; set; }

    public List<UserOptionViewModel> UserOptions { get; set; } = new();
}

public class ResidentDetailsViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? CitizenId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? EmergencyContact { get; set; }
    public string? Username { get; set; }
    public string? UserEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UserOptionViewModel
{
    public int UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
