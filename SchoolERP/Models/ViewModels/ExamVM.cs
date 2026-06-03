using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Models.ViewModels
{
    public class ExamVM
    {
        public int ExamId { get; set; }

        [Required]
        public string ExamName { get; set; }

        public string? Description { get; set; }

        [Required]
        public string ExamType { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required]
        public DateTime EndDate { get; set; } = DateTime.Today;
        public bool IsActive { get; set; }
        [Required]
        public int AcademicYearID { get; set; }

        [NotMapped]
        public string? AcademicYearName { get; set; }

        public List<SelectListItem> AcademicYears { get; set; } = new();
    }
}
