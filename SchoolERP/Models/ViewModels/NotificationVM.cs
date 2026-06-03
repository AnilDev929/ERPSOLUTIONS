using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolERP.Models.ViewModels
{
    public class NotificationVM
    {
        public int NotificationId { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }
        public string Priority { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<int> RoleIds { get; set; } = new();

        public List<SelectListItem> Roles { get; set; }
    }

    public class DashboardNotificationVM
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string TimeAgo { get; set; }
        public bool IsRead { get; set; }
        public string Priority { get; set; }
        public string Icon { get; set; }
        public string ColorClass { get; set; }
    }
}
