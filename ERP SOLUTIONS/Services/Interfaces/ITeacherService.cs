using ERP_SOLUTIONS.Models.DTOS;
using ERP_SOLUTIONS.Models.Entities;
using System.Runtime.CompilerServices;

namespace ERP_SOLUTIONS.Services.Interfaces
{
    public interface ITeacherService
    {
        Task<IEnumerable<TeacherDto>> GetAllAsync();
        Task<Teacher> GetByUserIdAsync(int userId);
        Task<Teacher> GetByIdAsync(int teacherId);

        Task<(string username, string password)> CreateTeacherWithUserAsync(Teacher teacher);
        Task<(bool Status, string message)> UpdateAsync(Teacher teacher, string Role);

        Task DeactivateAsync(int teacherId);


        // Teacher Methods (Limited Access)
        Task<TeacherProfileDTO> GetProfileByUserIdAsync(int userId);
        Task<bool> UpdateProfileAsync(TeacherProfileDTO model, int userId);
    }
}
