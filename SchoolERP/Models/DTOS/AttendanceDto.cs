using SchoolERP.Enum;

namespace SchoolERP.Models.DTOS
{
    public class AttendanceDto
    {
        public DateTime Date { get; set; }
        public string Subject { get; set; }
        public AttendanceStatus Status { get; set; }
    }
}
