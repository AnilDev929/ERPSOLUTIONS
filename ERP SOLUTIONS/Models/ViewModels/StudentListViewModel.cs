using ERP_SOLUTIONS.Models.DTOS;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ERP_SOLUTIONS.Models.ViewModels
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

