namespace SchoolERP.Models.DTOS
{
    public class NotificationDTO
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Priority { get; set; }

        public bool IsRead { get; set; } // 🔥 important
    }
}
