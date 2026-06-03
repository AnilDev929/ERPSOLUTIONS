namespace SchoolERP.Models.ViewModels
{
    public class NotificationBellVM
    {
        public int UnreadCount { get; set; }
        public List<NotificationItemVM> TopNotifications { get; set; }
    }

    public class NotificationItemVM
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
    }
}
