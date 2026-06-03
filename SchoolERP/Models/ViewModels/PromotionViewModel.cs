using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolERP.Models.DTOS;

namespace SchoolERP.Models.ViewModels
{
    public class PromotionViewModel
    {
        public List<SelectListItem> Classes { get; set; }
        public List<SelectListItem> Sections { get; set; }
        public List<SelectListItem> AcademicYears { get; set; }

        public int FromClassId { get; set; }
        public int FromSectionId { get; set; }
        public int FromAcademicYearId { get; set; }

        public int ToClassId { get; set; }
        public int ToSectionId { get; set; }
        public int ToAcademicYearId { get; set; }

        public List<int> StudentIds { get; set; } = new();

        public List<StudentDto> Students { get; set; } = new();

    }
}
