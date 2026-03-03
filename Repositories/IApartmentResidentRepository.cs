using BusinessObjects.Models;

namespace Repositories;

public interface IApartmentResidentRepository
{
    Task<ApartmentResident?> GetByIdAsync(int apartmentResidentId);
    Task<ApartmentResident?> GetActiveStayAsync(int residentId);
    Task<ApartmentResident?> GetActiveStayAsync(int apartmentId, int residentId);
    Task CreateAsync(ApartmentResident apartmentResident);
    Task UpdateAsync(ApartmentResident apartmentResident);
}
