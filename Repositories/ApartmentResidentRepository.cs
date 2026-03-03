using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class ApartmentResidentRepository : IApartmentResidentRepository
{
    private readonly AMSDbContext _dbContext;

    public ApartmentResidentRepository(AMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ApartmentResident?> GetByIdAsync(int apartmentResidentId)
    {
        return _dbContext.ApartmentResidents
            .Include(ar => ar.Apartment)
            .Include(ar => ar.Resident)
            .FirstOrDefaultAsync(ar => ar.Id == apartmentResidentId);
    }

    public Task<ApartmentResident?> GetActiveStayAsync(int residentId)
    {
        return _dbContext.ApartmentResidents
            .Include(ar => ar.Apartment)
            .FirstOrDefaultAsync(ar => ar.ResidentId == residentId && ar.IsActive);
    }

    public Task<ApartmentResident?> GetActiveStayAsync(int apartmentId, int residentId)
    {
        return _dbContext.ApartmentResidents
            .FirstOrDefaultAsync(ar => ar.ApartmentId == apartmentId && ar.ResidentId == residentId && ar.IsActive);
    }

    public async Task CreateAsync(ApartmentResident apartmentResident)
    {
        _dbContext.ApartmentResidents.Add(apartmentResident);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(ApartmentResident apartmentResident)
    {
        _dbContext.ApartmentResidents.Update(apartmentResident);
        await _dbContext.SaveChangesAsync();
    }
}
