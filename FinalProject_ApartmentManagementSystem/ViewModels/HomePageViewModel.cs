using System.Collections.Generic;

namespace FinalProject_ApartmentManagementSystem.ViewModels
{
    public class HomePageViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = "/images/default-avatar.png";
        public decimal WalletBalance { get; set; }
        public decimal AmountDue { get; set; }
        public int UnreadNotificationCount { get; set; }
        public ApartmentInfoViewModel? ApartmentInfo { get; set; }
        public List<AnnouncementViewModel> Announcements { get; set; } = new List<AnnouncementViewModel>();
        public List<NotificationItemViewModel> RecentNotifications { get; set; } = new List<NotificationItemViewModel>();
    }

    public class ApartmentInfoViewModel
    {
        public string ApartmentCode { get; set; } = string.Empty;
        public string Floor { get; set; } = string.Empty;
        public string BuildingName { get; set; } = string.Empty;
    }

    public class AnnouncementViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = "#";
    }

    public class NotificationItemViewModel
    {
        public int Id { get; set; }
        public bool IsRead { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string IconClass { get; set; } = "bi bi-bell";
        public string BadgeClass { get; set; } = "primary";
    }
}
