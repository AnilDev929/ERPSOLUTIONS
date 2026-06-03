using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.Entities
{
    public class Exam
    {
        public int ExamId { get; set; }

        [Required]
        [StringLength(150)]
        public string ExamName { get; set; }
        public string? Description { get; set; }
        public string ExamType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public int AcademicYearID { get; set; }
        public AcademicYear? AcademicYear { get; set; }
        //public bool IsActive { get; set; }
    }
}
