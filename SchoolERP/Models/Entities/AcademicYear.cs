using SchoolERP.Models.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.Entities
{
    public class AcademicYear
    {
        public int AcademicYearID { get; set; }

        [Required]
        [Display(Name = "Academic Year")]
        public string YearName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Year")]
        public DateTime YearStart { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Year")]
        public DateTime YearEnd { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<AcademicCalendarExceptions> Exceptions { get; set; }
    }
}
