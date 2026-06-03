namespace SchoolERP.Models.ViewModels
{
    public class TeacherHomeworkDashboardVM
    {
        public int TotalActive { get; set; }

        public int PendingReview { get; set; }

        public int SubmittedToday { get; set; }

        public int Overdue { get; set; }

        public List<TeacherHomeworkCardVM> Homeworks { get; set; }
    }

    public class TeacherHomeworkCardVM
    {
        public int HomeworkId { get; set; }

        public string SubjectName { get; set; }

        public string ClassName { get; set; }

        public string Title { get; set; }

        public DateTime DueDate { get; set; }

        public int SubmittedCount { get; set; }

        public int TotalStudents { get; set; }

        public string Status { get; set; }
    }



}
