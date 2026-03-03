using System.ComponentModel.DataAnnotations;

namespace FinalProject_ApartmentManagementSystem.ViewModels;

public class ApartmentsIndexViewModel
{
    public List<ApartmentListItemViewModel> Apartments { get; set; } = new();
}

public class ApartmentListItemViewModel
{
    public int Id { get; set; }
    public string ApartmentCode { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal? Area { get; set; }
    public int NumberOfRooms { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ActiveResidentCount { get; set; }
}

public class ApartmentFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Apartment code is required.")]
    [MaxLength(50)]
    public string ApartmentCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Building is required.")]
    public int BuildingId { get; set; }

    [Range(0, 200, ErrorMessage = "Floor must be 0 or greater.")]
    public int Floor { get; set; }

    [Range(0, 1000000, ErrorMessage = "Area must be positive.")]
    public decimal? Area { get; set; }

    [Range(1, 100, ErrorMessage = "Number of rooms must be at least 1.")]
    public int NumberOfRooms { get; set; } = 1;

    [MaxLength(50)]
    public string? ApartmentType { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public List<BuildingOptionViewModel> BuildingOptions { get; set; } = new();
    public List<string> StatusOptions { get; set; } = new();
}

public class BuildingOptionViewModel
{
    public int BuildingId { get; set; }
    public string BuildingName { get; set; } = string.Empty;
}

public class ApartmentDetailsViewModel
{
    public int Id { get; set; }
    public string ApartmentCode { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal? Area { get; set; }
    public int NumberOfRooms { get; set; }
    public string? ApartmentType { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<ApartmentResidentViewModel> Residents { get; set; } = new();
}

public class ApartmentResidentViewModel
{
    public int ApartmentResidentId { get; set; }
    public int ResidentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public DateOnly MoveInDate { get; set; }
    public DateOnly? MoveOutDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class AssignResidentViewModel
{
    public int ApartmentId { get; set; }
    public string ApartmentCode { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Resident is required.")]
    public int ResidentId { get; set; }

    [Required(ErrorMessage = "Move-in date is required.")]
    [DataType(DataType.Date)]
    public DateOnly MoveInDate { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? MoveOutDate { get; set; }

    public bool IsOwner { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public List<ResidentOptionViewModel> ResidentOptions { get; set; } = new();
}

public class ResidentOptionViewModel
{
    public int ResidentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
