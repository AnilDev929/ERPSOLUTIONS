using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolERP.Models.ViewModels
{
    public class AcademicCalendarExceptionListVM
    {

        public int ExceptionId { get; set; }
        public string AcademicYearName { get; set; }
        public string ExceptionType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsBillingBlocked { get; set; }
        public string? Description { get; set; } = string.Empty;
        
    }
}
