using BusinessObjects.Models;

namespace Repositories;

public interface IUserProfileRepository
{
    Task<User?> GetByIdAsync(int userId);
    Task UpdateAsync(User user);
}
