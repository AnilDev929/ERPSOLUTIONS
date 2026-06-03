using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.Entities
{
    public class ClassModel
    {
        [Key]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Class Name is required")]
        [StringLength(50)]
        [Display(Name = "Class Name")]
        public string ClassName { get; set; }
        public string? Description { get; set; }
        public int ClassOrder { get; set; }
    }
}
