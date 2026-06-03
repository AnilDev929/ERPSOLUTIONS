using SchoolERP.Enum;

namespace SchoolERP.Models.DTOS
{
    public class UpdateTeacherStatusDto
    {
        public int TeacherID { get; set; }
        public TeacherStatus Status { get; set; } = TeacherStatus.Active;
        public string StatusRemark { get; set; }
    }
}
