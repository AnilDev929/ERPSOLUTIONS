namespace SchoolERP.Models.Entities
{
    public class ExamTimeSlot
    {
        public int ExamTimeSlotId { get; set; }

        public string? ExamType { get; set; }   // Unit Test, Midterm, Final

        public int? ExamId { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        // Not stored in DB (UI helper property)
        public string SlotName
        {
            get
            {
                //return $"{StartDate:hh\\:mm tt} - {EndDate:hh\\:mm tt}";
                return $"{DateTime.Today.Add(StartTime):hh:mm tt} - {DateTime.Today.Add(EndTime):hh:mm tt}";
            }
        }

        public int DurationMinutes
        {
            get
            {
                return (int)(EndTime - StartTime).TotalMinutes;
            }
        }

        public int SortOrder { get; set; }
        public string? FloorNo { get; set; } = "";
        public string? RoomNo { get; set; } = "";
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
