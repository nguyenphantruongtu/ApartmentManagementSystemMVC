namespace Services
{
    public interface IAuthService
    {
    Task<LoginResult> LoginAsync(LoginRequest request);
    Task<RegisterResult> RegisterAsync(RegisterRequest request);
    Task RequestPasswordResetAsync(ForgotPasswordRequest request);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request);
    Task<OperationResult> ChangePasswordAsync(ChangePasswordRequest request);
    }
}
