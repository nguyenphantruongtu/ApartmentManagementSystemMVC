using System;
using System.ComponentModel.DataAnnotations;

namespace FinalProject_ApartmentManagementSystem.ViewModels;

public class RolesIndexViewModel
{
    public List<RoleListItemViewModel> Roles { get; set; } = new();
}

public class RoleListItemViewModel
{
    public int Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UserCount { get; set; }
}

public class RoleFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Role name is required.")]
    [MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }
}

public class UsersIndexViewModel
{
    public List<UserRoleListItemViewModel> Users { get; set; } = new();
}

public class UserRoleListItemViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
}

public class RoleAssignmentViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public List<RoleOptionViewModel> RoleOptions { get; set; } = new();
    public List<int> SelectedRoleIds { get; set; } = new();
}

public class RoleOptionViewModel
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
