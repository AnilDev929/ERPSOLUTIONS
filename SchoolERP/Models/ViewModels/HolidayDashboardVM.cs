using SchoolERP.Models.Entities;

namespace SchoolERP.Models.ViewModels
{
    public class HolidayDashboardVM
    {
        public int TotalHolidays { get; set; }
        public int UpcomingHolidays { get; set; }
        public int VacationDays { get; set; }
        public int RecurringHolidays { get; set; }

        public List<Holiday> Holidays { get; set; }
        public Holiday? NextHoliday { get; set; }
    }
}
