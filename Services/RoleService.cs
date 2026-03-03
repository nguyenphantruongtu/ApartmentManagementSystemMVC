using BusinessObjects.Models;
using Repositories;

namespace Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<IReadOnlyList<Role>> GetRolesAsync()
    {
        return await _roleRepository.GetRolesAsync();
    }

    public Task<Role?> GetRoleAsync(int roleId)
    {
        return _roleRepository.GetRoleByIdAsync(roleId);
    }

    public async Task<OperationResult> CreateRoleAsync(string roleName, string? description)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return OperationResult.Failure("Role name is required.");
        }

        var existing = await _roleRepository.GetRoleByNameAsync(roleName);
        if (existing is not null)
        {
            return OperationResult.Failure("Role name already exists.");
        }

        await _roleRepository.CreateRoleAsync(new Role
        {
            RoleName = roleName.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
        });

        return OperationResult.Success();
    }

    public async Task<OperationResult> UpdateRoleAsync(int roleId, string roleName, string? description)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return OperationResult.Failure("Role name is required.");
        }

        var role = await _roleRepository.GetRoleByIdAsync(roleId);
        if (role is null)
        {
            return OperationResult.Failure("Role not found.");
        }

        var existing = await _roleRepository.GetRoleByNameAsync(roleName);
        if (existing is not null && existing.Id != roleId)
        {
            return OperationResult.Failure("Role name already exists.");
        }

        role.RoleName = roleName.Trim();
        role.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        await _roleRepository.UpdateRoleAsync(role);

        return OperationResult.Success();
    }

    public async Task<OperationResult> DeleteRoleAsync(int roleId)
    {
        var role = await _roleRepository.GetRoleByIdAsync(roleId);
        if (role is null)
        {
            return OperationResult.Failure("Role not found.");
        }

        await _roleRepository.DeleteRoleAsync(role);
        return OperationResult.Success();
    }

    public async Task<IReadOnlyList<User>> GetUsersWithRolesAsync()
    {
        return await _roleRepository.GetUsersWithRolesAsync();
    }

    public Task<User?> GetUserWithRolesAsync(int userId)
    {
        return _roleRepository.GetUserWithRolesAsync(userId);
    }

    public async Task<OperationResult> UpdateUserRolesAsync(int userId, IReadOnlyCollection<int> roleIds)
    {
        var user = await _roleRepository.GetUserWithRolesAsync(userId);
        if (user is null)
        {
            return OperationResult.Failure("User not found.");
        }

        await _roleRepository.UpdateUserRolesAsync(userId, roleIds);
        return OperationResult.Success();
    }
}
