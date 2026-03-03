using System.Security.Claims;
using FinalProject_ApartmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace FinalProject_ApartmentManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserProfileService _userProfileService;

        public AccountController(IAuthService authService, IUserProfileService userProfileService)
        {
            _authService = authService;
            _userProfileService = userProfileService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            model.ReturnUrl = NormalizeReturnUrl(model.ReturnUrl);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loginResult = await _authService.LoginAsync(
                new LoginRequest(model.Username, model.Password));

            if (!loginResult.Succeeded || loginResult.User is null)
            {
                ModelState.AddModelError(string.Empty, loginResult.ErrorMessage ?? "Đăng nhập thất bại.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, loginResult.User.Id.ToString()),
                new(ClaimTypes.Name, loginResult.User.Username),
                new(ClaimTypes.Email, loginResult.User.Email),
                new("full_name", loginResult.User.FullName)
            };

            foreach (var role in loginResult.User.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authenticationProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authenticationProperties);

            return RedirectToLocal(model.ReturnUrl);
        }

        [HttpPost]
        [Authorize(Policy = "AnyRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult PendingApproval()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await _authService.RegisterAsync(new RegisterRequest
            (
                model.Username, model.Email, model.Password, model.FullName, model.Phone
            ));
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Đăng ký thất bại.");
                return View(model);
            }
            return View("PendingApproval");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _authService.RequestPasswordResetAsync(new ForgotPasswordRequest(model.Email));
            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? email, string? token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                ModelState.AddModelError(string.Empty, "Reset token is invalid.");
            }

            return View(new ResetPasswordViewModel
            {
                Email = email ?? string.Empty,
                Token = token ?? string.Empty
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.ResetPasswordAsync(
                new ResetPasswordRequest(model.Email, model.Token, model.Password));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Reset password failed.");
                return View(model);
            }

            return View("ResetPasswordConfirmation");
        }

        [HttpGet]
        [Authorize(Policy = "AnyRole")]
        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Challenge();
            }

            var profile = await _userProfileService.GetProfileAsync(userId.Value);
            if (profile is null)
            {
                return NotFound();
            }

            return View(new UserProfileViewModel
            {
                Username = profile.Username,
                Email = profile.Email,
                FullName = profile.FullName,
                Phone = profile.Phone,
                AvatarUrl = profile.AvatarUrl
            });
        }

        [HttpPost]
        [Authorize(Policy = "AnyRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                var existing = await _userProfileService.GetProfileAsync(userId.Value);
                if (existing is not null)
                {
                    model.Username = existing.Username;
                    model.Email = existing.Email;
                }
                return View(model);
            }

            var result = await _userProfileService.UpdateProfileAsync(new UpdateUserProfileRequest(
                userId.Value,
                model.FullName,
                model.Phone,
                model.AvatarUrl));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to update profile.");
                return View(model);
            }

            await RefreshSignInAsync(model.FullName, model.Email);
            TempData["ProfileUpdated"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [Authorize(Policy = "AnyRole")]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize(Policy = "AnyRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.ChangePasswordAsync(
                new ChangePasswordRequest(userId.Value, model.CurrentPassword, model.NewPassword));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to change password.");
                return View(model);
            }

            TempData["PasswordChanged"] = "Password changed successfully.";
            return RedirectToAction(nameof(Profile));
        }


        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        private string? NormalizeReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            return null;
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }

        private async Task RefreshSignInAsync(string fullName, string email)
        {
            var currentUser = User;
            if (currentUser.Identity?.IsAuthenticated != true)
            {
                return;
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, currentUser.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty),
                new(ClaimTypes.Name, currentUser.FindFirstValue(ClaimTypes.Name) ?? string.Empty),
                new(ClaimTypes.Email, email),
                new("full_name", fullName)
            };

            var roleClaims = currentUser.FindAll(ClaimTypes.Role);
            claims.AddRange(roleClaims);

            var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var properties = authResult.Properties ?? new AuthenticationProperties();

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                properties);
        }
    }
}
