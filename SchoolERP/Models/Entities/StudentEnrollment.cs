using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Models.Entities
{
    public class StudentEnrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        public int SectionId { get; set; }

        [Required]
        public int AcademicYearId { get; set; }

        [Required]
        public int RollNumber { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active / Promoted / Left

        public bool IsActive { get; set; } = true;
        public bool IsPromoted { get; set; } = false;
        public bool IsNewAdmission { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties (optional but recommended)
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("ClassId")]
        public virtual ClassModel Class { get; set; }

        [ForeignKey("SectionId")]
        public virtual Section Section { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; }
    }
}
