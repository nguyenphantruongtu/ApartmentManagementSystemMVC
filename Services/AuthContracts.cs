namespace Services;

public sealed record LoginRequest(string UsernameOrEmail, string Password);

public sealed record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FullName,
    string? Phone);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword);

public sealed record ChangePasswordRequest(
    int UserId,
    string CurrentPassword,
    string NewPassword);

public sealed record AuthenticatedUserDto(
    int Id,
    string Username,
    string Email,
    string FullName,
    string[] Roles);

public sealed record LoginResult(
    bool Succeeded,
    AuthenticatedUserDto? User = null,
    string? ErrorMessage = null);

public sealed record RegisterResult(
    bool Succeeded,
    AuthenticatedUserDto? User = null,
    string? ErrorMessage = null);

public sealed record OperationResult(
    bool Succeeded,
    string? ErrorMessage = null)
{
    public static OperationResult Success() => new(true);
    public static OperationResult Failure(string errorMessage) => new(false, errorMessage);
}
