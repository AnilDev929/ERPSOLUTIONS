using SchoolERP.Models.ViewModels;

namespace SchoolERP.Services.Interfaces
{
    public interface IMarksService
    {
        List<StudentMarkRowVM> GetStudents(int classId, int sectionId);
        void SaveMarks(MarksEntryVM vm);
    }
}
