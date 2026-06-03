using SchoolERP.Models.DTOS;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolERP.Models.ViewModels
{
    public class StudentListViewModel
    {
        public int? ClassID { get; set; }
        public int? SectionID { get; set; }
        public string? RollNumber { get; set; }

        public List<SelectListItem> Classes { get; set; } = new();
        public List<SelectListItem> Sections { get; set; } = new();

        public List<StudentDto> Students { get; set; } = new();
    }
}

