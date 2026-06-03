using SchoolERP.Models.Entities;

namespace SchoolERP.Models.ViewModels
{
    public class HomeworkDetailsViewModel
    {
        public int HomeworkId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Instructions { get; set; }

        public string SubjectName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string TeacherName { get; set; }

        public DateTime AssignedDate { get; set; }
        public DateTime DueDate { get; set; }

        public decimal? TotalMarks { get; set; }

        public string Priority { get; set; }
        public string Status { get; set; }
        public string HomeworkType { get; set; }

        public bool AllowLateSubmission { get; set; }

        //public List<string> Attachments { get; set; } = new();

        public List<HomeworkAttachment> Attachments { get; set; }

    }
}
