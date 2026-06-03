using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Models.Entities
{
    public class StudentPromotionLog
    {
        [Key]
        public int PromotionId { get; set; }

        [Required]
        public int StudentId { get; set; }

        public int? FromClassId { get; set; }
        public int? ToClassId { get; set; }

        public int? FromSectionId { get; set; }
        public int? ToSectionId { get; set; }

        public int? FromAcademicYearId { get; set; }
        public int? ToAcademicYearId { get; set; }

        public int? OldEnrollmentId { get; set; }
        public int? NewEnrollmentId { get; set; }

        public int? RollNumber { get; set; }

        public DateTime PromotionDate { get; set; } = DateTime.Now;

        public int? PromotedBy { get; set; } // Admin/Teacher ID

        [MaxLength(255)]
        public string Remarks { get; set; }

        // Navigation property
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        // Optional navigation properties (only if you have these entities)
        [ForeignKey("FromClassId")]
        public virtual ClassModel FromClass { get; set; }

        [ForeignKey("ToClassId")]
        public virtual ClassModel ToClass { get; set; }

        [ForeignKey("FromSectionId")]
        public virtual Section FromSection { get; set; }

        [ForeignKey("ToSectionId")]
        public virtual Section ToSection { get; set; }

        [ForeignKey("FromAcademicYearId")]
        public virtual AcademicYear FromAcademicYear { get; set; }

        [ForeignKey("ToAcademicYearId")]
        public virtual AcademicYear ToAcademicYear { get; set; }
    }
}
