namespace SchoolERP.Models.DTOS
{
    public class TeacherLockDTO
    {
        public int TeacherID { set; get; }
        public int UserID { set; get; }
        public string UserName { set; get; }
        public string FullName { set; get; }
        public string MobileNo { set; get; }
        public string EmailID { set; get; }
        public string LastLogin { set; get; }
        public int FailedCount { set; get; }
        public bool IsActive { set; get; }
        public bool IsLocked { set; get; }
    }
}
