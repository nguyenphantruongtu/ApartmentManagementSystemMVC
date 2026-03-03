using BusinessObjects.Models;

namespace Repositories;

public interface IAuthRepository
{
    Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task<Role> EnsureRoleAsync(string roleName);
    Task AssignRoleAsync(int userId, int roleId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int userId);
    Task AddPasswordResetTokenAsync(int userId, string token, DateTime expiresAtUtc);
    Task<RefreshToken?> GetValidPasswordResetTokenAsync(int userId, string token, DateTime utcNow);
    Task RevokePasswordResetTokensAsync(int userId);
    Task UpdateUserAsync(User user);
}
