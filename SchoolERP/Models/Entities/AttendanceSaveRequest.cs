namespace SchoolERP.Models.Entities
{
    public class AttendanceSaveRequest
    {
        public int SubjectId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public Dictionary<int, string> Records { get; set; } = new();
    }
}
