using Repositories;

namespace Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;

    public UserProfileService(IUserProfileRepository userProfileRepository)
    {
        _userProfileRepository = userProfileRepository;
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var user = await _userProfileRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        return new UserProfileDto(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.Phone,
            user.AvatarUrl);
    }

    public async Task<OperationResult> UpdateProfileAsync(UpdateUserProfileRequest request)
    {
        var user = await _userProfileRepository.GetByIdAsync(request.UserId);
        if (user is null)
        {
            return OperationResult.Failure("User was not found.");
        }

        user.FullName = request.FullName.Trim();
        user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _userProfileRepository.UpdateAsync(user);
        return OperationResult.Success();
    }
}
