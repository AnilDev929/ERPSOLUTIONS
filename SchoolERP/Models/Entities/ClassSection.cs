using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Models.Entities
{
    public class ClassSection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        public int SectionId { get; set; }

        // CLASS TEACHER
        public int? ClassTeacherId { get; set; }

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual ClassModel Class { get; set; }

        [ForeignKey("SectionId")]
        public virtual Section Section { get; set; }

        //Used for to assign teacher to section
        public Teacher? ClassTeacher { get; set; }
        // Optional: list of students in this class-section
        public virtual ICollection<Student> Students { get; set; }
        public ICollection<ClassSectionSubject> ClassSectionSubjects { get; set; }
    }
}
