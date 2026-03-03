using BusinessObjects.Models;

namespace Repositories;

public interface IResidentRepository
{
    Task<List<Resident>> GetResidentsAsync();
    Task<Resident?> GetResidentByIdAsync(int residentId);
    Task<Resident?> GetResidentDetailAsync(int residentId);
    Task<Resident?> GetResidentByCitizenIdAsync(string citizenId);
    Task<Resident?> GetResidentByUserIdAsync(int userId);
    Task<Resident> CreateResidentAsync(Resident resident);
    Task UpdateResidentAsync(Resident resident);
    Task DeleteResidentAsync(Resident resident);
}
