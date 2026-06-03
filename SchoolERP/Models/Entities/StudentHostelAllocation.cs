using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Models.Entities
{
    public class StudentHostelAllocation
    {
        [Key]
        public int AllocationId { get; set; }

        // Foreign Key: Student
        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        // Foreign Key: Hostel
        [Required]
        public int HostelId { get; set; }

        [ForeignKey("HostelId")]
        public virtual Hostel Hostel { get; set; }

        // Foreign Key: Room
        [Required]
        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }

        [Required]
        public DateTime AllocationDate { get; set; } = DateTime.Now;

        // Optional end date (null if still active)
        public DateTime? ReleaseDate { get; set; }

        // Indicates whether student is currently occupying the room
        public bool IsActive { get; set; } = true;

        // Optional notes
        [MaxLength(250)]
        public string Notes { get; set; }
    }
}
