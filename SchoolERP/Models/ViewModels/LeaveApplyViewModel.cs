using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;

namespace SchoolERP.Models.ViewModels
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
        // File Upload
        public IFormFile? LeaveDocument { get; set; }
        public List<LeaveTypeViewModel> leaveSummery { get; set; } = new();
        [ValidateNever]
        public List<LeaveHistoryDto> LeaveHistory { get; set; }
        public decimal TotalLeaves { get; set; }
    }
}
