using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AMSDbContext _dbContext;

    public AuthRepository(AMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
    {
        var normalized = usernameOrEmail.Trim().ToLowerInvariant();

        return await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == normalized || u.Email.ToLower() == normalized);
    }

    public Task<bool> UsernameExistsAsync(string username)
    {
        var normalized = username.Trim().ToLowerInvariant();
        return _dbContext.Users.AnyAsync(u => u.Username.ToLower() == normalized);
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _dbContext.Users.AnyAsync(u => u.Email.ToLower() == normalized);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<Role> EnsureRoleAsync(string roleName)
    {
        var normalized = roleName.Trim().ToLowerInvariant();

        var existingRole = await _dbContext.Roles.FirstOrDefaultAsync(
            r => r.RoleName.ToLower() == normalized);
        if (existingRole is not null)
        {
            return existingRole;
        }

        var role = new Role
        {
            RoleName = roleName.Trim()
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        return role;
    }

    public async Task AssignRoleAsync(int userId, int roleId)
    {
        var alreadyAssigned = await _dbContext.UserRoles.AnyAsync(
            ur => ur.UserId == userId && ur.RoleId == roleId);

        if (alreadyAssigned)
        {
            return;
        }

        _dbContext.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();

        return await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized);
    }

    public Task<User?> GetUserByIdAsync(int userId)
    {
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task AddPasswordResetTokenAsync(
        int userId,
        string token,
        DateTime expiresAtUtc)
    {
        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAtUtc,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }

    public Task<RefreshToken?> GetValidPasswordResetTokenAsync(
        int userId,
        string token,
        DateTime utcNow)
    {
        return _dbContext.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.UserId == userId
                && rt.Token == token
                && !rt.IsRevoked
                && rt.ExpiresAt > utcNow);
    }

    public async Task RevokePasswordResetTokensAsync(int userId)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        if (activeTokens.Count == 0)
        {
            return;
        }

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }
}
