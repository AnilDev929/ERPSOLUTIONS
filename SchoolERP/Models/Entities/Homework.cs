using SchoolERP.Enum;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.Entities
{
    public class Homework
    {
        public int HomeworkId { get; set; }

        public int ClassId { get; set; }

        public int? SectionId { get; set; }

        public int SubjectId { get; set; }

        public int TeacherId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(200)]
        public string? Instructions { get; set; }
        
        public DateTime AssignedDate { get; set; }

        public DateTime DueDate { get; set; }

        public decimal? TotalMarks { get; set; }

        //public byte HomeworkType { get; set; }
        public HomeworkType HomeworkType { get; set; }

        public HomeworkStatus Status { get; set; } = HomeworkStatus.Active;

        // Optional
        public string? AttachmentPath { get; set; }

        // Optional metadata
        public HomeworkPriority Priority { get; set; } = HomeworkPriority.Medium;

        public bool AllowLateSubmission { get; set; } = false;

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }


        // Navigation Properties

        public virtual ClassModel Class { get; set; }

        public virtual Section Section { get; set; }

        public virtual Subject Subject { get; set; }

        public virtual Teacher Teacher { get; set; }

        // ✅ ADD THIS (IMPORTANT FIX)
        public virtual ICollection<HomeworkSubmission> HomeworkSubmissions { get; set; }
            = new List<HomeworkSubmission>();
        public virtual ICollection<HomeworkAttachment> HomeworkAttachments { get; set; }
    }


}
