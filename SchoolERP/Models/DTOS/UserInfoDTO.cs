namespace SchoolERP.Models.DTOS
{
    public class UserInfoDTO
    {
        public int UserID { get; set; }
        public int RoleID { set; get; }
        public string UserName { get; set; }
        = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string CreatedDate { set; get; } = "";
        public string LastLogin { set; get; } = "";
        public UserInfoDTO() { }
    }
}
