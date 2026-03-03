using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Apartment
{
    public int Id { get; set; }

    public string ApartmentCode { get; set; } = null!;

    public int BuildingId { get; set; }

    public int Floor { get; set; }

    public decimal? Area { get; set; }

    public int NumberOfRooms { get; set; }

    public string? ApartmentType { get; set; }

    public string Status { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ApartmentResident> ApartmentResidents { get; set; } = new List<ApartmentResident>();

    public virtual Building Building { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();
}
