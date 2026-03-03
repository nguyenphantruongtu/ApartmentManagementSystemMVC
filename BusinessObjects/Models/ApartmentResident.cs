using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class ApartmentResident
{
    public int Id { get; set; }

    public int ApartmentId { get; set; }

    public int ResidentId { get; set; }

    public bool IsOwner { get; set; }

    public DateOnly MoveInDate { get; set; }

    public DateOnly? MoveOutDate { get; set; }

    public bool IsActive { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Apartment Apartment { get; set; } = null!;

    public virtual Resident Resident { get; set; } = null!;
}
