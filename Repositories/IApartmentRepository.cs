using BusinessObjects.Models;

namespace Repositories;

public interface IApartmentRepository
{
    Task<List<Apartment>> GetApartmentsAsync();
    Task<Apartment?> GetApartmentByIdAsync(int apartmentId);
    Task<Apartment?> GetApartmentDetailAsync(int apartmentId);
    Task<Apartment?> GetApartmentByCodeAsync(string apartmentCode);
    Task<Building?> GetBuildingByIdAsync(int buildingId);
    Task<List<Building>> GetBuildingsAsync();
    Task<Apartment> CreateApartmentAsync(Apartment apartment);
    Task UpdateApartmentAsync(Apartment apartment);
    Task DeleteApartmentAsync(Apartment apartment);
}
