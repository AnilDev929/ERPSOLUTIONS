using ERP_SOLUTIONS.Enum;

namespace ERP_SOLUTIONS.Models.Entities
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; }   // ✅ Add this
        //public Teacher Teacher { get; set; }   // ✅ Correct navigation
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public bool IsHalfDay { get; set; }
        public string? HalfDayType { get; set; } // FirstHalf / SecondHalf

        public decimal TotalDays { get; set; }

        public string? Reason { get; set; }

        public LeaveStatus Status { get; set; } // Pending / Approved / Rejected / Cancelled

        public int? ApprovedBy { get; set; } // Principal
        public DateTime? ApprovedDate { get; set; }
    }
}
