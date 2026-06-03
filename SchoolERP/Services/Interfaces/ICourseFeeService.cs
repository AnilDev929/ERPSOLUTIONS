using SchoolERP.Models.Entities;

namespace SchoolERP.Services.Interfaces
{
    public interface ICourseFeeService
    {
        Task<List<object>> GetAllAsync(int? classId, int? yearId);
        Task<CourseFee> GetByIdAsync(int id);
        Task<(bool success, string message)> SaveAsync(CourseFee model);
        Task<(bool success, string message)> DeleteAsync(int id);
        Task<object> GetDropdownsAsync();
    }
}
