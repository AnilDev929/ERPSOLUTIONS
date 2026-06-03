using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.ViewModels
{
    public class FeeConfigurationVM
    {
        public int ConfigId { get; set; }

        [Required(ErrorMessage = "Academic Year is required")]
        public int AcademicYearID { get; set; }

        [Required]
        public string FeeType { get; set; }

        [Required]
        [Range(1, 31, ErrorMessage = "Enter valid day (1-31)")]
        public int DueDay { get; set; }

        public int GraceDays { get; set; }

        [Required]
        public string LateFineType { get; set; }

        [Required]
        public decimal LateFineAmount { get; set; }

        public int MaxMonthsPerPayment { get; set; }

        public bool IsActive { get; set; }

        // Dropdowns
        public List<SelectListItem> FeeTypes { get; set; }
        public List<SelectListItem> LateFineTypes { get; set; }
    }
}
