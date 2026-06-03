using SchoolERP.Models.Entities;

namespace SchoolERP.Services.Interfaces
{
    public interface IClassService
    {
        Task<IEnumerable<ClassModel>> GetAllClassesAsync();
        Task<ClassModel> GetClassByIdAsync(int id);
        Task<bool> AddClassAsync(ClassModel newClass);
        Task<bool> UpdateClassAsync(ClassModel updatedClass);
        Task<bool> DeleteClassAsync(int id);
    }
}
