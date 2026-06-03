namespace SchoolERP.Models.Entities
{
    public class AcademicCalendarExceptions
    {
        public int ExceptionId { get; set; }

        public int AcademicYearId { get; set; }

        public string ExceptionType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsBillingBlocked { get; set; } = true;

        public string? Description { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        // Navigation
        public AcademicYear AcademicYear { get; set; }
    }
}
