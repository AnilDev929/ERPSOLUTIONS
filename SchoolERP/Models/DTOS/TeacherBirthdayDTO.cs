namespace SchoolERP.Models.DTOS
{
    public class TeacherBirthdayDTO
    {
        public int TeacherId { get; set; }

        public string TeacherName { get; set; }
        public string Designation  { get; set; }
        public string Gender { get; set; }

        public DateTime DOB { get; set; }

        public DateTime UpcomingDate { get; set; }

        public int DaysRemaining { get; set; }
    }
}
