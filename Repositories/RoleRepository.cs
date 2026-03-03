using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AMSDbContext _dbContext;

    public RoleRepository(AMSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Role>> GetRolesAsync()
    {
        return _dbContext.Roles
            .Include(r => r.UserRoles)
            .OrderBy(r => r.RoleName)
            .ToListAsync();
    }

    public Task<Role?> GetRoleByIdAsync(int roleId)
    {
        return _dbContext.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public Task<Role?> GetRoleByNameAsync(string roleName)
    {
        var normalized = roleName.Trim().ToLowerInvariant();
        return _dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == normalized);
    }

    public async Task<Role> CreateRoleAsync(Role role)
    {
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        return role;
    }

    public async Task UpdateRoleAsync(Role role)
    {
        _dbContext.Roles.Update(role);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteRoleAsync(Role role)
    {
        if (role.UserRoles.Count > 0)
        {
            _dbContext.UserRoles.RemoveRange(role.UserRoles);
        }

        _dbContext.Roles.Remove(role);
        await _dbContext.SaveChangesAsync();
    }

    public Task<List<User>> GetUsersWithRolesAsync()
    {
        return _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    public Task<User?> GetUserWithRolesAsync(int userId)
    {
        return _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task UpdateUserRolesAsync(int userId, IReadOnlyCollection<int> roleIds)
    {
        var existing = await _dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        var currentRoleIds = existing.Select(ur => ur.RoleId).ToHashSet();
        var desiredRoleIds = roleIds.ToHashSet();

        var toRemove = existing.Where(ur => !desiredRoleIds.Contains(ur.RoleId)).ToList();
        if (toRemove.Count > 0)
        {
            _dbContext.UserRoles.RemoveRange(toRemove);
        }

        var toAdd = desiredRoleIds.Except(currentRoleIds).ToList();
        foreach (var roleId in toAdd)
        {
            _dbContext.UserRoles.Add(new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync();
    }
}
