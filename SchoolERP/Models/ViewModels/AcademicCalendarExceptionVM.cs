using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolERP.Models.ViewModels
{
    public class AcademicCalendarExceptionVM
    {
        public int? ExceptionId { get; set; }

        public int AcademicYearId { get; set; }

        public string ExceptionType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsBillingBlocked { get; set; } = true;

        public string Description { get; set; }

        public List<SelectListItem> AcademicYears { get; set; }
    }
}
