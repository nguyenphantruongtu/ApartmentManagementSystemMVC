using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class ApartmentRepository : IApartmentRepository
{
    private readonly AMSDbContext _dbContext;

    public ApartmentRepository(AMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Apartment>> GetApartmentsAsync()
    {
        return _dbContext.Apartments
            .Include(a => a.Building)
            .Include(a => a.ApartmentResidents)
            .OrderBy(a => a.ApartmentCode)
            .ToListAsync();
    }

    public Task<Apartment?> GetApartmentByIdAsync(int apartmentId)
    {
        return _dbContext.Apartments
            .Include(a => a.Building)
            .FirstOrDefaultAsync(a => a.Id == apartmentId);
    }

    public Task<Apartment?> GetApartmentDetailAsync(int apartmentId)
    {
        return _dbContext.Apartments
            .Include(a => a.Building)
            .Include(a => a.ApartmentResidents)
            .ThenInclude(ar => ar.Resident)
            .FirstOrDefaultAsync(a => a.Id == apartmentId);
    }

    public Task<Apartment?> GetApartmentByCodeAsync(string apartmentCode)
    {
        var normalized = apartmentCode.Trim().ToLowerInvariant();
        return _dbContext.Apartments
            .FirstOrDefaultAsync(a => a.ApartmentCode.ToLower() == normalized);
    }

    public Task<Building?> GetBuildingByIdAsync(int buildingId)
    {
        return _dbContext.Buildings.FirstOrDefaultAsync(b => b.Id == buildingId);
    }

    public Task<List<Building>> GetBuildingsAsync()
    {
        return _dbContext.Buildings
            .OrderBy(b => b.BuildingName)
            .ToListAsync();
    }

    public async Task<Apartment> CreateApartmentAsync(Apartment apartment)
    {
        _dbContext.Apartments.Add(apartment);
        await _dbContext.SaveChangesAsync();
        return apartment;
    }

    public async Task UpdateApartmentAsync(Apartment apartment)
    {
        _dbContext.Apartments.Update(apartment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteApartmentAsync(Apartment apartment)
    {
        _dbContext.Apartments.Remove(apartment);
        await _dbContext.SaveChangesAsync();
    }
}
