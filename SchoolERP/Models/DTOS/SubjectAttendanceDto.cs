namespace SchoolERP.Models.DTOS
{
    public class SubjectAttendanceDto
    {
        public string Subject { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public decimal Percentage { get; set; }
    }
}
