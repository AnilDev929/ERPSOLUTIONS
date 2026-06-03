namespace SchoolERP.Models.DTOS
{
    public class StudentDto
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public string RollNumber { get; set; }
        public string AdmissionNumber { get; set; }
        public int Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string EmilID { get; set; }
        public string JoinDate { get; set; }

        public int ClassSectionID { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public int SectionID { get; set; }
        public string SectionName { get; set; }

        public decimal AttendancePercentage { get; set; }
        public string ProfileImagePath { get; set; }
        public decimal PendingFees { get; set; }

        public string FeeStatus { get; set; }
        public bool IsActive { get; set; }
    }

    public class NewStudentDto
    {
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public int StudentID { get; set; }
        public string AdmissionNumber { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public DateTime JoinDate { get; set; }
    }

}
