using BusinessObjects.Models;

namespace Services;

public interface IRoleService
{
    Task<IReadOnlyList<Role>> GetRolesAsync();
    Task<Role?> GetRoleAsync(int roleId);
    Task<OperationResult> CreateRoleAsync(string roleName, string? description);
    Task<OperationResult> UpdateRoleAsync(int roleId, string roleName, string? description);
    Task<OperationResult> DeleteRoleAsync(int roleId);
    Task<IReadOnlyList<User>> GetUsersWithRolesAsync();
    Task<User?> GetUserWithRolesAsync(int userId);
    Task<OperationResult> UpdateUserRolesAsync(int userId, IReadOnlyCollection<int> roleIds);
}
