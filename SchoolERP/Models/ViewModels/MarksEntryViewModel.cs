using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolERP.Models.ViewModels
{
    public class MarksEntryViewModel
    {
        public int? ClassId { get; set; }
        public int? SectionId { get; set; }
        public int? SubjectId { get; set; }
        public int? ExamId { get; set; }

        public List<StudentMarksRow> Students { get; set; }

        public SelectList Classes { get; set; }
        public SelectList Sections { get; set; }
        public SelectList Subjects { get; set; }
        public SelectList Exams { get; set; }
    }


    public class StudentMarksRow
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string Grade  {get; set; }
        public bool IsAbsent { get; set; } = false;
        public string? Remark { get; set; } = "";
        public int? MarksObtained { get; set; }
    }
}
