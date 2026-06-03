namespace SchoolERP.Models.DTOS
{
    public class UpcomingBirthdayDTO
    {
        public int StudentId { get; set; }

        public string StudentName { get; set; }
        public string Gender { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }

        public DateTime DOB { get; set; }
        public int DaysRemaining { get; set; }
        public DateTime UpcomingDate { get; set; }
    }
}
