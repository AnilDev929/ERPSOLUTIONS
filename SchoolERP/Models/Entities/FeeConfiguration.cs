using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.Entities
{
    public class FeeConfiguration
    {
        public int ConfigId { get; set; }

        [Required]
        public int AcademicYearID { get; set; }

        [Required]
        [StringLength(20)]
        public string FeeType { get; set; } // Monthly / Quarterly

        [Range(1, 31)]
        public int DueDay { get; set; }

        [Range(0, 30)]
        public int GraceDays { get; set; } = 0;

        [Required]
        public string LateFineType { get; set; } // MONTHLY / DAILY / SLAB

        [Range(0, 10000)]
        public decimal LateFineAmount { get; set; }

        [Range(1, 12)]
        public int MaxMonthsPerPayment { get; set; } = 3;

        public bool IsActive { get; set; } = true;
    }
}
