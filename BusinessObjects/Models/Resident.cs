using BusinessObjects.Models;

public partial class Resident
{
    public int Id { get; set; }

    // Nullable: NULL = cư dân phụ không có tài khoản
    public int? UserId { get; set; }

    public string FullName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? CitizenId { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? EmergencyContact { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ApartmentResident> ApartmentResidents { get; set; } = new List<ApartmentResident>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    // Nullable vì UserId có thể NULL
    public virtual User? User { get; set; }
}