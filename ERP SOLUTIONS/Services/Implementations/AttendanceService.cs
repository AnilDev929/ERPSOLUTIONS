using ERP_SOLUTIONS.Enum;
using ERP_SOLUTIONS.Models.ViewModels;
using ERP_SOLUTIONS.Services.Interfaces;

namespace ERP_SOLUTIONS.Services.Implementations
{
    public class AttendanceService : IAttendanceService
    {
        // In-memory storage for demo
        private static List<(int StudentId, int SubjectId, DateTime Date, AttendanceStatus Status)> _attendanceRecords
            = new List<(int, int, DateTime, AttendanceStatus)>();

        // Save attendance
        public void SaveAttendance(int studentId, int subjectId, DateTime date, AttendanceStatus status)
        {
            // Remove existing record if any
            _attendanceRecords.RemoveAll(a => a.StudentId == studentId && a.SubjectId == subjectId && a.Date == date);

            // Add new record
            _attendanceRecords.Add((studentId, subjectId, date, status));
        }

        // Get attendance for subject & date
        public List<StudentViewModel> GetAttendanceBySubject(int subjectId, DateTime date)
        {
            // For demo, we just return some dummy students
            var demoStudents = new List<StudentViewModel>
        {
            new StudentViewModel { Id = 1, Name = "John Doe" },
            new StudentViewModel { Id = 2, Name = "Jane Smith" },
            new StudentViewModel { Id = 3, Name = "Michael Johnson" },
            new StudentViewModel { Id = 4, Name = "Emily Davis" },
        };

            // Map attendance if exists
            foreach (var student in demoStudents)
            {
                var record = _attendanceRecords.FirstOrDefault(a => a.StudentId == student.Id && a.SubjectId == subjectId && a.Date.Date == date.Date);
                if (record != default)
                    student.Status = record.Status;
            }

            return demoStudents;
        }
    }
}