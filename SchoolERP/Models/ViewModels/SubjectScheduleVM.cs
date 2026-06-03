namespace SchoolERP.Models.ViewModels
{
    public class SubjectScheduleVM
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }

        public DateTime ExamDate { get; set; }
        //public TimeSpan? StartDate { get; set; }
        //public TimeSpan? EndDate { get; set; }
        public int MaxMarks { get; set; }
        public int PassingMarks { get; set; }
        public int ExamTimeSlotId { get; set; }
    }
}
