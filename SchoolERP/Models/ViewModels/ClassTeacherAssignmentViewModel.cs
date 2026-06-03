using SchoolERP.Models.Entities;

namespace SchoolERP.Models.ViewModels
{
    public class TeacherVm
    {
        public int TeacherId { get; set; }

        public string Name { get; set; }

        public string Initials { get; set; }

        public string SubjectName { get; set; }

        public int TotalAssignedSections { get; set; }
    }

    public class SectionCardVm
    {
        public int ClassSectionId { get; set; }

        public string ClassName { get; set; }

        public string SectionName { get; set; }

        public int StudentCount { get; set; }

        public TeacherVm? ClassTeacher { get; set; }
    }

    public class ClassTeacherAssignmentViewModel
    {
        public List<ClassModel> Classes { get; set; } = new();
        public List<SectionCardVm> Sections { get; set; } = new();
        public List<TeacherVm> Teachers { get; set; } = new();
    }
}
