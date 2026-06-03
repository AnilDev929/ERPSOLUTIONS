using SchoolERP.Enum;

namespace SchoolERP.Models.DTOS
{
    public class TeacherDto
    {
        public int TeacherID { get; set; }
        public string TeacherName { get; set; }
        public string Designation { get; set; }

        public string Qualification { get; set; } = string.Empty;
        public string Specialization { set; get; }
        public string Gender { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
        public TeacherStatus Status { get; set; } = TeacherStatus.Active;
    }
}
