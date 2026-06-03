using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.ViewModels
{
    public class StudentHomeworkVM
    {
        public int HomeworkId { get; set; }

        public string SubjectName { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime DueDate { get; set; }

        public string Status { get; set; }

        public int DaysLeft { get; set; }

        public bool IsSubmitted { get; set; }
    }

    public class SubmitHomeworkVM
    {
        public int HomeworkId { get; set; }

        public string Title { get; set; }

        public string SubjectName { get; set; }

        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Please enter remarks")]
        [StringLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters")]
        public string Remarks { get; set; }

        [Required(ErrorMessage = "Please upload a file")]
        public IFormFile File { get; set; }
    }
}
