using SchoolERP.Models.Entities;

namespace SchoolERP.Services.Interfaces
{
    public interface ISubjectService
    {
        Task<bool> AddSubjectAsync(Subject subject);
        Task<List<Subject>> GetAllSubjectsAsync();
        Task<Subject> GetSubjectByIdAsync(int id);
        Task<bool> UpdateSubjectAsync(Subject subject);
        Task<bool> DeleteSubjectAsync(int id);
    }
}
