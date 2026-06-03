using SchoolERP.Models.DTOS;

namespace SchoolERP.Models.ViewModels
{
    public class StudentAttendanceVM
    {
        public decimal OverallPercentage { get; set; }
        public decimal MonthPercentage { get; set; }
        public decimal WeekPercentage { get; set; }

        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }

        public List<AttendanceDto> AttendanceList { get; set; }
        public List<SubjectAttendanceDto> SubjectStats { get; set; }
    }
}
