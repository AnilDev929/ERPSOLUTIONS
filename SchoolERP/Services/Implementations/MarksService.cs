using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;

namespace SchoolERP.Services.Implementations
{
    public class MarksService : IMarksService
    {
        private readonly AppDbContext _db;

        public MarksService(AppDbContext db)
        {
            _db = db;
        }

        public List<StudentMarkRowVM> GetStudents(int classId, int sectionId)
        {
            var data = (from s in _db.Students
                               join cs in _db.ClassSections
                                   on s.ClassSectionId equals cs.Id
                               where cs.ClassId == classId
                                     && cs.SectionId == sectionId
                               select new StudentMarkRowVM
                               {
                                   StudentId = s.StudentID,
                                   StudentName = s.StudentName
                               }).ToList();

            return data;
        }

        public void SaveMarks(MarksEntryVM vm)
        {
            var maxMarks = _db.ExamSchedules
                .Where(x => x.ClassId == vm.ClassId && x.SubjectId == vm.SubjectId && x.ExamId == vm.ExamId)
                .Select(x => x.MaxMarks)
                .FirstOrDefault();

            foreach (var s in vm.Students)
            {
                var existing = _db.StudentMarks.FirstOrDefault(x =>
                    x.StudentId == s.StudentId &&
                    x.ClassId == vm.ClassId &&
                    x.SectionId == vm.SectionId &&
                    x.SubjectId == vm.SubjectId &&
                    x.ExamId == vm.ExamId);

                if (existing != null)
                {
                    existing.MarksObtained = s.MarksObtained;
                }
                else
                {
                    _db.StudentMarks.Add(new StudentMarks
                    {
                        StudentId = s.StudentId,
                        ClassId = vm.ClassId ?? 0, // vm.ClassId.Value,
                        SectionId = vm.SectionId ?? 0, // vm.SectionId.Value,
                        SubjectId = vm.SubjectId ?? 0, // vm.SubjectId.Value,
                        ExamId = vm.ExamId ?? 0, //vm.ExamId.Value
                        MarksObtained = s.MarksObtained,
                        MaxMarks = maxMarks
                    });
                }
            }

            _db.SaveChanges();
        }
    }
}
