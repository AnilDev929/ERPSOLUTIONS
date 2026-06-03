using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;

namespace SchoolERP.Models.ViewModels
{
    public class ParentDashboardViewModel
    {
        public List<StudentDto> Students { get; set; }

        //public List<Homework> Homework { get; set; }

        public List<ExamSchedule> Exams { get; set; }

        //public List<TeacherRemarks> Remarks { get; set; }

        public List<NotificationDTO> Notifications { get; set; }

        public List<FeePayment> FeeHistory { get; set; }
    }
}
