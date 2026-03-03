using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly AMSDbContext _dbContext;

    public UserProfileRepository(AMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(int userId)
    {
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task UpdateAsync(User user)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }
}
