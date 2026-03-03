using System.Security.Cryptography;
using System.Text;
using BusinessObjects.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Repositories;

namespace Services
{
    public class AuthService : IAuthService
    {
        private const string DefaultRoleName = "Resident";

        private readonly IAuthRepository _authRepository;
        private readonly IPasswordHasherService _passwordHasherService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IAuthRepository authRepository,
            IPasswordHasherService passwordHasherService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _authRepository = authRepository;
            _passwordHasherService = passwordHasherService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            var user = await _authRepository.GetUserByUsernameOrEmailAsync(request.UsernameOrEmail);
            if (user is null || !user.IsActive)
            {
                return new LoginResult(false, ErrorMessage: "Invalid credentials.");
            }

            var isValidPassword = _passwordHasherService.VerifyPassword(request.Password, user.PasswordHash);
            if (!isValidPassword)
            {
                return new LoginResult(false, ErrorMessage: "Invalid credentials.");
            }

            return new LoginResult(true, MapToAuthenticatedUser(user));
        }

        public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
        {
            if (await _authRepository.UsernameExistsAsync(request.Username))
            {
                return new RegisterResult(false, ErrorMessage: "Username is already taken.");
            }

            if (await _authRepository.EmailExistsAsync(request.Email))
            {
                return new RegisterResult(false, ErrorMessage: "Email is already registered.");
            }

            var now = DateTime.UtcNow;
            var user = new User
            {
                Username = request.Username.Trim(),
                Email = request.Email.Trim(),
                FullName = string.IsNullOrWhiteSpace(request.FullName) ? request.Username.Trim() : request.FullName.Trim(),
                PasswordHash = _passwordHasherService.HashPassword(request.Password),
                Phone = request.Phone.Trim(),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            user = await _authRepository.CreateUserAsync(user);

            var defaultRole = await _authRepository.EnsureRoleAsync(DefaultRoleName);
            await _authRepository.AssignRoleAsync(user.Id, defaultRole.Id);

            var freshUser = await _authRepository.GetUserByEmailAsync(user.Email) ?? user;
            return new RegisterResult(true, MapToAuthenticatedUser(freshUser));
        }

        public async Task RequestPasswordResetAsync(ForgotPasswordRequest request)
        {
            var user = await _authRepository.GetUserByEmailAsync(request.Email);
            if (user is null)
            {
                return;
            }

            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
            var tokenHash = HashToken(rawToken);
            var tokenMinutes = int.TryParse(_configuration["Auth:PasswordReset:TokenMinutes"], out var minutes) ? minutes : 30;
            var expiresAt = DateTime.UtcNow.AddMinutes(Math.Max(5, tokenMinutes));

            await _authRepository.AddPasswordResetTokenAsync(user.Id, tokenHash, expiresAt);

            var resetUrl = BuildResetUrl(request.Email, rawToken);
            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                user.FullName,
                resetUrl);
        }

        public async Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _authRepository.GetUserByEmailAsync(request.Email);
            if (user is null)
            {
                return OperationResult.Failure("Invalid reset token.");
            }

            var tokenHash = HashToken(request.Token);
            var token = await _authRepository.GetValidPasswordResetTokenAsync(
                user.Id,
                tokenHash,
                DateTime.UtcNow);

            if (token is null)
            {
                return OperationResult.Failure("Invalid or expired reset token.");
            }

            user.PasswordHash = _passwordHasherService.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _authRepository.UpdateUserAsync(user);
            await _authRepository.RevokePasswordResetTokensAsync(user.Id);

            return OperationResult.Success();
        }

        public async Task<OperationResult> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = await _authRepository.GetUserByIdAsync(request.UserId);
            if (user is null)
            {
                return OperationResult.Failure("User was not found.");
            }

            if (!_passwordHasherService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return OperationResult.Failure("Current password is incorrect.");
            }

            user.PasswordHash = _passwordHasherService.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _authRepository.UpdateUserAsync(user);
            await _authRepository.RevokePasswordResetTokensAsync(user.Id);

            return OperationResult.Success();
        }

        private string BuildResetUrl(string email, string token)
        {
            var configuredBaseUrl = _configuration["Auth:PasswordReset:ResetBaseUrl"]?.TrimEnd('/');
            var resetPath = _configuration["Auth:PasswordReset:ResetPath"] ?? "/Account/ResetPassword";
            var escapedEmail = Uri.EscapeDataString(email);
            var escapedToken = Uri.EscapeDataString(token);

            if (string.IsNullOrWhiteSpace(configuredBaseUrl))
            {
                _logger.LogWarning("Auth:PasswordReset:ResetBaseUrl is not configured, using relative reset link.");
                return $"{resetPath}?email={escapedEmail}&token={escapedToken}";
            }

            return $"{configuredBaseUrl}{resetPath}?email={escapedEmail}&token={escapedToken}";
        }

        private static string HashToken(string token)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashBytes);
        }

        private static AuthenticatedUserDto MapToAuthenticatedUser(User user)
        {
            var roleNames = user.UserRoles
                .Select(ur => ur.Role?.RoleName)
                .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new AuthenticatedUserDto(
                user.Id,
                user.Username,
                user.Email,
                user.FullName,
                roleNames);
        }
    }
}
