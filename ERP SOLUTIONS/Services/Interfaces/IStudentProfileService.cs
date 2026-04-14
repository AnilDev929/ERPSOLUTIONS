using ERP_SOLUTIONS.Models.DTOS;
using ERP_SOLUTIONS.Models.Entities;
using ERP_SOLUTIONS.Models.ViewModels;

namespace ERP_SOLUTIONS.Services.Interfaces
{
    public interface IStudentProfileService
    {
        Task<EditStudentProfileDto> GetStudentProfileData(int id);
        Task<CreateStudentResultDto> SaveStudentDetail(StudentFormViewModel student);
        Task<StudentProfileDto> GetStudentProfileAsync(int StudentID);
        Task UpdateStudentProfileAsync(EditStudentProfileDto model);
    }
}
