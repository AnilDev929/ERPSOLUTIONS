using ERP_SOLUTIONS.Enum;

namespace ERP_SOLUTIONS.Models.DTOS
{
    public class LeaveApprovalDto
    {
        public int Id { get; set; }
        public string TeacherName { get; set; }
        public string LeaveType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalDays { get; set; }
        public string Reason { get; set; }
        public LeaveStatus Status { get; set; } = LeaveStatus.Approved;
    }
}
