using ERP_SOLUTIONS.Enum;
using ERP_SOLUTIONS.Models.ViewModels;

namespace ERP_SOLUTIONS.Services.Interfaces
{
    public interface IAttendanceService
    { 
        // Save attendance for a student
        void SaveAttendance(int studentId, int subjectId, DateTime date, AttendanceStatus status);

        // Get attendance for a subject and date
        List<StudentViewModel> GetAttendanceBySubject(int subjectId, DateTime date);
    }
}
