using SchoolERP.Common;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using System.Runtime.CompilerServices;

namespace SchoolERP.Services.Interfaces
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

        Task<ServiceResponse<TeacherProfileUpdateDTO>> GetProfileAsync(int teacherId);
        Task<ServiceResponse<TeacherProfileUpdateDTO>> UpdateProfileAsync(TeacherProfileUpdateDTO model, int userId);



    }
}
