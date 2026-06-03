using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolERP.Enum;
using SchoolERP.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.ViewModels
{
    public class CreateHomeworkVM
    {
        public int? HomeworkId { get; set; }

        [Required]
        public int ClassId { get; set; }

        public int? SectionId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(5000)]
        public string Description { get; set; }

        public string Instructions { get; set; }

        [Required]
        public DateTime AssignedDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public decimal? TotalMarks { get; set; }

        public HomeworkPriority Priority { get; set; } = HomeworkPriority.Medium;

        //public byte HomeworkType { get; set; }

        public HomeworkType HomeworkType { get; set; }

        public bool AllowLateSubmission { get; set; }

        public List<IFormFile> Files { get; set; }

        // Dropdowns
        public IEnumerable<SelectListItem>? ClassList { get; set; }


        public IEnumerable<SelectListItem>? SectionList { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem>? SubjectList { get; set; } = new List<SelectListItem>();

        public List<HomeworkAttachment>? ExistingAttachments { get; set; } = new();
    }
}
