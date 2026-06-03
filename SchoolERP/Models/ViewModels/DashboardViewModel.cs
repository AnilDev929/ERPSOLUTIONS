using SchoolERP.Models.DTOS;

namespace SchoolERP.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int StudentCount { get; set; }
        public int TeacherCount { get; set; }
        public int ClassCount { get; set; }
        public int SubjectCount { get; set; } = 0;
        public int TotalHolidays { get; set; } = 0;
        public decimal TodayAttendance { get; set; } = 0;
        public int NewAdmissionsThisMonth { get; set; } = 0;
        
        public int NewStudentCount { get; set; } = 0;
        public decimal AttendancePercentage { get; set; }
        public int UpcomingExamCount { get; set; }
        public decimal PendingFees { get; set; } = 0;
        public int ExamCount { get; set; } = 0;

        public List<DashboardNotificationVM> Notifications { get; set; }
        public List<UpcomingBirthdayDTO> UpcomingBirthdays { get; set; }
        public List<TeacherBirthdayDTO> UpcomingTeacherBirthDay { get; set; }
        // BADGES
        public int StudentGrowthPercentage { get; set; }
        public bool TeacherActiveStatus { get; set; }

        // CHART DATA
        public List<string> ChartLabels { get; set; } = new();
        public List<int> ChartStudentData { get; set; } = new();
    }
}
