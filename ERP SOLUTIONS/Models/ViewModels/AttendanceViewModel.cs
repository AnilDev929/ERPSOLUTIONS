namespace ERP_SOLUTIONS.Models.ViewModels
{
    public class AttendanceViewModel
    {
        public int SubjectId { get; set; }

        public DateTime Date { get; set; }

        public List<StudentViewModel> Students { get; set; }
    }
}
