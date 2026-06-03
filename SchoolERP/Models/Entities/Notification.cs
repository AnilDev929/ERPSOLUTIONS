
namespace SchoolERP.Models.Entities
{
    public class Notification
    {
        public int NotificationId { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Priority { get; set; } // Low, Medium, High

        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public List<NotificationTarget> Targets { get; set; }
       
    }

    public class NotificationTarget
    {
        public int TargetId { get; set; }
        public int NotificationId { get; set; }
        public int RoleId { get; set; }
    }

    public class NotificationReadStatus
    {
        public int ReadId { get; set; }
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsRead { get; set; } = false;
    }

}
