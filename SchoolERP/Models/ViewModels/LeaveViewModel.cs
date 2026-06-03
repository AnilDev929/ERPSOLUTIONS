using SchoolERP.Enum;

namespace SchoolERP.Models.ViewModels
{
    public class LeaveViewModel
    {
        public int Id { get; set; }
        public string ApplicantName { get; set; } // Teacher or Student
        public string LeaveType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalDays { get; set; }
        public string Reason { get; set; }
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    }
}
