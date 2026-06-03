using SchoolERP.Enum;

namespace SchoolERP.Models.ViewModels
{
    public class StudentViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        // Default = Present
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    }
}
