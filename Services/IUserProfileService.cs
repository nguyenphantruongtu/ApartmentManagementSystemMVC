namespace Services;

public interface IUserProfileService
{
    Task<UserProfileDto?> GetProfileAsync(int userId);
    Task<OperationResult> UpdateProfileAsync(UpdateUserProfileRequest request);
}
