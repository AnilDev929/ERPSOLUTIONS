namespace SchoolERP.Models.Entities
{
    public class ChangePasswordResult
    {
        public bool Succeeded { get; set; }
        public string[] Errors { get; set; }
    }
}
