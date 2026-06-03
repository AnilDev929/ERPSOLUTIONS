using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Models.Entities
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
        public decimal? AdmissionFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TransportFee { set; get; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? LibraryFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? OtherFee { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // Computed property (not stored in DB by default)
        [NotMapped]
        public decimal TotalFee => TuitionFee + (AdmissionFee ?? 0) + (TransportFee ?? 0) + (LibraryFee ?? 0) + (OtherFee ?? 0);

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual ClassModel Class { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; }
    }
}
