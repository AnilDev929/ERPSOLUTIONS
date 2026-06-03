using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolERP.Models.ViewModels
{
    public class ExamScheduleViewModel
    {
        public int? Id { get; set; }
        public int ExamId { get; set; }
        public int ClassId { get; set; }
        public int SectionId { get; set; }
        public List<SubjectScheduleVM> Subjects { get; set; }



        public List<SelectListItem> Exams { get; set; }
        public List<SelectListItem> Classes { get; set; }
    }
}
