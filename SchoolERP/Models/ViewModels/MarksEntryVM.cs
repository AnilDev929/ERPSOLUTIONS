using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolERP.Models.ViewModels
{
    public class MarksEntryVM
    {
        public int? ClassId { get; set; }
        public int? SectionId { get; set; }
        public int? SubjectId { get; set; }
        public int? ExamId { get; set; }

        public List<StudentMarkRowVM> Students { get; set; } = new();

        public SelectList Classes { get; set; }
        public SelectList Exams { get; set; }
    }
}
