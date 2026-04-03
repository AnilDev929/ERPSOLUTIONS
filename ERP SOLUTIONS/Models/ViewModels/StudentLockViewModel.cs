namespace ERP_SOLUTIONS.Models.ViewModels
{
    public class StudentLockViewModel
    {
        public int StudentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public bool IsLocked { get; set; }
    }
}
