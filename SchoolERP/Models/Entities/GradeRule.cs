using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Models.Entities
{
    //public class GradeRule
    //{
    //    public int GradeRuleID { get; set; }
    //    [Required]
    //    public string RuleName { get; set; }
    //    [Required]
    //    [Range(0, 100)]
    //    public decimal MinScore { get; set; }
    //    [Required]
    //    [Range(0, 100)]
    //    public decimal MaxScore { get; set; }
    //}


    [Table("GradeRule")]
    public class GradeRule
    {
        [Key]
        public int GradeRuleID { get; set; }

        [Required]
        [StringLength(100)]
        public string RuleName { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal MinScore { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal MaxScore { get; set; }

        [Required]
        [StringLength(5)]
        public string GradeLetter { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal? GradePoint { get; set; }

        [StringLength(10)]
        public string? ResultStatus { get; set; }  // PASS / FAIL

        public int? DisplayOrder { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public bool IsFail { get; set; } = false;

        [StringLength(10)]
        public string? ColorCode { get; set; }
    }
}
