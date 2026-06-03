using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SchoolERP.Models.Entities
{
    public class StudentMarks
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        public int SectionId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public int ExamId { get; set; }

        [Range(0, 1000, ErrorMessage = "Marks must be valid")]
        public decimal? MarksObtained { get; set; }

        [Required]
        [Range(1, 1000)]
        public int MaxMarks { get; set; }

        public bool IsAbsent { get; set; } = false;
        public string? Grade {  get; set; }  


        [MaxLength(250)]
        public string? Remarks { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; }

        // 🔗 Navigation Properties
        public virtual Student Student { get; set; }
        public virtual ClassModel Class { get; set; }
        public virtual Section Section { get; set; }
        public virtual Subject Subject { get; set; }
        public virtual Exam Exam { get; set; }
    }
}
