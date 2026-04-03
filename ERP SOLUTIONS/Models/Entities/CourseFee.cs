using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_SOLUTIONS.Models.Entities
{
    public class CourseFee
    {
        [Key]
        public int CourseFeeId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        public int AcademicYearId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TuitionFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? LabFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? LibraryFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? OtherFee { get; set; }

        // Computed property (not stored in DB by default)
        [NotMapped]
        public decimal TotalFee => TuitionFee + (LabFee ?? 0) + (LibraryFee ?? 0) + (OtherFee ?? 0);

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual ClassModel Class { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; }
    }
}
