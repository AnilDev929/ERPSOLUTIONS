namespace SchoolERP.Models.ViewModels
{
    public class StudentLockViewModel
    {
        public int StudentId { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string LastLogin { get; set; }
        public int FailedCount { get; set; }
        public bool IsLocked { get; set; }
    }
}
