namespace SchoolERP.Models.ViewModels
{
    public class ParentUnlockViewModel
    {
        public int ParentId { get; set; }
        public string ParentName { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string LockReason { get; set; } = string.Empty;
        public List<StudentChildViewModel> Children { get; set; } = new();
    }

    public class StudentChildViewModel 
    { 
        public int StudentId { get; set; } 
        public string RollNumber { get; set; }
        public string StudentName { get; set; } 
        public string ClassName { get; set; } 
        public string SectionName { get; set; } }
}
