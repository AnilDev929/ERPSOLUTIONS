using SchoolERP.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Models.Entities
{
    public class Attendance
    {
        [Key]
        public int AttendanceID { get; set; }

        [Required]
        public int StudentId { get; set; }

        public int? TeacherID { get; set; } // nullable

        [Required]
        public int SubjectID { get; set; }

        [Required]
        public DateTime AttendanceDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [StringLength(10)]
        public AttendanceStatus Status { get; set; }

        // 🔗 Navigation Properties
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("TeacherID")]
        public virtual Teacher Teacher { get; set; }

        [ForeignKey("SubjectID")]
        public virtual Subject Subject { get; set; }
    }
}
