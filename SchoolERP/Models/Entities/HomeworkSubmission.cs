using SchoolERP.Enum;

namespace SchoolERP.Models.Entities
{
    public class HomeworkSubmission
    {
        public int HomeworkSubmissionId { get; set; }

        public int HomeworkId { get; set; }

        public int StudentId { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public string SubmissionText { get; set; }

        public string AttachmentPath { get; set; }

        public bool IsLateSubmission { get; set; }

        public SubmissionStatus Status { get; set; }

        public decimal? MarksObtained { get; set; }

        public string TeacherRemarks { get; set; }
        public DateTime ReviewedAt { get; set; }
        // Navigation
        public virtual Homework Homework { get; set; }

        public virtual Student Student { get; set; }
    }
}
