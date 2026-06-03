using SchoolERP.Enum;

namespace SchoolERP.Models.DTOS
{
    public class AttendanceDashboardDto
    {
        public DateTime AttendanceDate { get; set; }

        public int TotalStudents { get; set; }

        public int PresentStudents { get; set; }

        public int AbsentStudents { get; set; }

        public decimal AttendancePercentage { get; set; }

        public List<ClassAttendanceReportDto> Classes { get; set; }
    }

    public class ClassAttendanceReportDto
    {
        public int ClassId { get; set; }

        public string ClassName { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public int Strength { get; set; }

        public int PresentCount { get; set; }

        public int AbsentCount { get; set; }

        public decimal AttendancePercentage { get; set; }

        public string Status { get; set; } = "Present";
    }


    public class StudentAttendanceDto
    {
        public string StudentId { get; set; }
        public DateTime Date { get; set; }
        
        public string StudentName { get; set; }

        public int RollNo { get; set; }

        public AttendanceStatus Status { get; set; }
    }
}
