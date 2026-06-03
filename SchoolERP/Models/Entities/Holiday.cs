using SchoolERP.Enum;

namespace SchoolERP.Models.Entities
{
    public class Holiday
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } // supports multi-day holidays

        public bool IsFullDay { get; set; } = true;

        public HolidayType Type { get; set; } // Enum

        public bool IsRecurring { get; set; } // repeats yearly

        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
        public int AcademicYearId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
