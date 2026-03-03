using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class ResidentRepository : IResidentRepository
{
    private readonly AMSDbContext _dbContext;

    public ResidentRepository(AMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Resident>> GetResidentsAsync()
    {
        return _dbContext.Residents
            .OrderBy(r => r.FullName)
            .ToListAsync();
    }

    public Task<Resident?> GetResidentByIdAsync(int residentId)
    {
        return _dbContext.Residents.FirstOrDefaultAsync(r => r.Id == residentId);
    }

    public Task<Resident?> GetResidentDetailAsync(int residentId)
    {
        return _dbContext.Residents
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == residentId);
    }

    public Task<Resident?> GetResidentByCitizenIdAsync(string citizenId)
    {
        var normalized = citizenId.Trim();
        return _dbContext.Residents.FirstOrDefaultAsync(r => r.CitizenId == normalized);
    }

    public Task<Resident?> GetResidentByUserIdAsync(int userId)
    {
        return _dbContext.Residents.FirstOrDefaultAsync(r => r.UserId == userId);
    }

    public async Task<Resident> CreateResidentAsync(Resident resident)
    {
        _dbContext.Residents.Add(resident);
        await _dbContext.SaveChangesAsync();
        return resident;
    }

    public async Task UpdateResidentAsync(Resident resident)
    {
        _dbContext.Residents.Update(resident);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteResidentAsync(Resident resident)
    {
        _dbContext.Residents.Remove(resident);
        await _dbContext.SaveChangesAsync();
    }
}
