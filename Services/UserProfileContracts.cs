namespace Services;

public record UserProfileDto(
    int UserId,
    string Username,
    string Email,
    string FullName,
    string? Phone,
    string? AvatarUrl);

public record UpdateUserProfileRequest(
    int UserId,
    string FullName,
    string? Phone,
    string? AvatarUrl);
