using System.Diagnostics;
using System.Security.Claims;
using BusinessObjects.Models;
using FinalProject_ApartmentManagementSystem.Models;
using FinalProject_ApartmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ApartmentManagementSystem.Controllers
{
    [Authorize(Policy = "AnyRole")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AMSDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, AMSDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var user = userId.HasValue
                ? await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId.Value)
                : null;

            var fullName = user?.FullName
                ?? User.FindFirstValue("full_name")
                ?? User.Identity?.Name
                ?? string.Empty;

            var avatarUrl = string.IsNullOrWhiteSpace(user?.AvatarUrl)
                ? "/images/default-avatar.png"
                : user.AvatarUrl;

            Resident? resident = null;
            if (userId.HasValue)
            {
                resident = await _dbContext.Residents.AsNoTracking()
                    .FirstOrDefaultAsync(r => r.UserId == userId.Value);
            }

            ApartmentInfoViewModel? apartmentInfo = null;
            if (resident is not null)
            {
                var activeStay = await _dbContext.ApartmentResidents.AsNoTracking()
                    .Include(ar => ar.Apartment)
                    .ThenInclude(a => a.Building)
                    .Where(ar => ar.ResidentId == resident.Id && ar.IsActive)
                    .OrderByDescending(ar => ar.MoveInDate)
                    .FirstOrDefaultAsync();

                if (activeStay?.Apartment is not null)
                {
                    apartmentInfo = new ApartmentInfoViewModel
                    {
                        ApartmentCode = activeStay.Apartment.ApartmentCode,
                        Floor = $"Tang {activeStay.Apartment.Floor}",
                        BuildingName = activeStay.Apartment.Building.BuildingName
                    };
                }
            }

            decimal walletBalance = 0;
            decimal amountDue = 0;
            if (resident is not null)
            {
                walletBalance = await _dbContext.Payments.AsNoTracking()
                    .Where(p => p.Invoice.ResidentId == resident.Id)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                amountDue = await _dbContext.Invoices.AsNoTracking()
                    .Where(i => i.ResidentId == resident.Id)
                    .SumAsync(i => (decimal?)Math.Max(0, i.TotalAmount - i.PaidAmount)) ?? 0;
            }

            var unreadCount = 0;
            var notifications = new List<NotificationItemViewModel>();
            if (userId.HasValue)
            {
                unreadCount = await _dbContext.Notifications.AsNoTracking()
                    .CountAsync(n => n.UserId == userId.Value && !n.IsRead);

                var recent = await _dbContext.Notifications.AsNoTracking()
                    .Where(n => n.UserId == userId.Value)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                notifications = recent.Select(n =>
                {
                    var (icon, badge) = GetNotificationStyle(n.Type);
                    return new NotificationItemViewModel
                    {
                        Id = n.Id,
                        IsRead = n.IsRead,
                        Title = n.Title,
                        Content = n.Content,
                        TimeAgo = FormatTimeAgo(n.CreatedAt),
                        IconClass = icon,
                        BadgeClass = badge
                    };
                }).ToList();
            }

            var model = new HomePageViewModel
            {
                FullName = fullName,
                AvatarUrl = avatarUrl,
                WalletBalance = walletBalance,
                AmountDue = amountDue,
                UnreadNotificationCount = unreadCount,
                ApartmentInfo = apartmentInfo,
                RecentNotifications = notifications
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }

        private static string FormatTimeAgo(DateTime createdAt)
        {
            var delta = DateTime.UtcNow - createdAt;
            if (delta.TotalMinutes < 1)
            {
                return "Vua xong";
            }

            if (delta.TotalHours < 1)
            {
                return $"{Math.Max(1, (int)delta.TotalMinutes)} phut truoc";
            }

            if (delta.TotalDays < 1)
            {
                return $"{Math.Max(1, (int)delta.TotalHours)} gio truoc";
            }

            if (delta.TotalDays < 30)
            {
                return $"{Math.Max(1, (int)delta.TotalDays)} ngay truoc";
            }

            return createdAt.ToString("dd/MM/yyyy");
        }

        private static (string IconClass, string BadgeClass) GetNotificationStyle(string? type)
        {
            return type?.Trim().ToLowerInvariant() switch
            {
                "invoice" => ("bi bi-receipt", "warning"),
                "payment" => ("bi bi-check-circle", "success"),
                "issue" => ("bi bi-tools", "primary"),
                "system" => ("bi bi-info-circle", "primary"),
                "alert" => ("bi bi-exclamation-triangle", "danger"),
                _ => ("bi bi-bell", "primary")
            };
        }
    }
}
