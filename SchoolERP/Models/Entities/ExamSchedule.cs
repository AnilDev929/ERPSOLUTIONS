

namespace SchoolERP.Models.Entities
{
    public class ExamSchedule
    {
        public int ScheduleId { get; set; }
        public int ExamId { get; set; }
        public Exam Exam { get; set; }
        public int ClassId { get; set; }
        public ClassModel Class { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
        public DateTime ExamDate { get; set; }
        public int MaxMarks { get; set; }
        public int PassingMarks { get; set; }
        public int ExamTimeSlotId { get; set; }
        public ExamTimeSlot TimeSlot { get; set; }
    }
}
