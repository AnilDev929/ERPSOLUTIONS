using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_SOLUTIONS.Models.Entities
{
    public class ClassSection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        public int SectionId { get; set; }

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual ClassModel Class { get; set; }

        [ForeignKey("SectionId")]
        public virtual Section Section { get; set; }

        // Optional: list of students in this class-section
        public virtual ICollection<Student> Students { get; set; }
    }
}
