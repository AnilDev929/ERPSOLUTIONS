using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_SOLUTIONS.Models.Entities
{
    public class StudentFee
    {
        [Key]
        public int FeeId { get; set; }

        public int? StudentId { get; set; }

        [Required]
        public int AcademicYearID { get; set; }

        public int? CourseFeeId { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentOption { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountApplied { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalFee { get; set; }

        public int? Months { get; set; }

        public int? PaidMonths { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MonthlyAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal RemainingAmount { get; set; } = 0;

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public bool? IncludeTransport { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "DUE";

        // Navigation Properties
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("AcademicYearID")]
        public virtual AcademicYear AcademicYear { get; set; }

        [ForeignKey("CourseFeeId")]
        public virtual CourseFee CourseFee { get; set; }
    }
}
