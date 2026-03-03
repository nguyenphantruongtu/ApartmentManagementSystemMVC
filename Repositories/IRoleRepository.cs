using BusinessObjects.Models;

namespace Repositories;

public interface IRoleRepository
{
    Task<List<Role>> GetRolesAsync();
    Task<Role?> GetRoleByIdAsync(int roleId);
    Task<Role?> GetRoleByNameAsync(string roleName);
    Task<Role> CreateRoleAsync(Role role);
    Task UpdateRoleAsync(Role role);
    Task DeleteRoleAsync(Role role);
    Task<List<User>> GetUsersWithRolesAsync();
    Task<User?> GetUserWithRolesAsync(int userId);
    Task UpdateUserRolesAsync(int userId, IReadOnlyCollection<int> roleIds);
}
