namespace SchoolERP.Models.ViewModels
{
    public class StudentExamScheduleVM
    {
        public int ScheduleId { get; set; }

        public int ExamId { get; set; }
        public string ExamName { get; set; }

        public int SubjectId { get; set; }
        public string SubjectName { get; set; }

        public DateTime ExamDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string SlotName { get; set; }
        
        public string RoomNo { get; set; }
        public string FloorNo { get; set; }

        public int MaxMarks { get; set; }
        public int PassingMarks { get; set; }
    }
}
