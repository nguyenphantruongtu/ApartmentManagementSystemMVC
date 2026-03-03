using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Building
{
    public int Id { get; set; }

    public string BuildingCode { get; set; } = null!;

    public string BuildingName { get; set; } = null!;

    public string? Address { get; set; }

    public int TotalFloors { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();
}
