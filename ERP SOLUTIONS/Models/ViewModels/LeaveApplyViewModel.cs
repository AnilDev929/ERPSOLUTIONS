using ERP_SOLUTIONS.Models.DTOS;
using ERP_SOLUTIONS.Models.Entities;

namespace ERP_SOLUTIONS.Models.ViewModels
{
    public class LeaveApplyViewModel
    {
        public string? LeaveType { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;
        public bool IsHalfDay { get; set; } = false;
        public string? HalfDayType { get; set; }
        public int DaysTaken { get; set; } = 0;
        public string Reason { get; set; }

        public List<LeaveTypeViewModel> leaveSummery { get; set; } = new();
        public List<LeaveHistoryDto> LeaveHistory { get; set; }
        public decimal TotalLeaves { get; set; }
    }
}
