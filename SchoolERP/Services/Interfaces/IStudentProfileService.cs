using SchoolERP.Models.DTOS;
using SchoolERP.Models.ViewModels;

namespace SchoolERP.Services.Interfaces
{
    public interface IStudentProfileService
    {
        Task<EditStudentProfileDto> GetStudentProfileData(int id);
        Task<CreateStudentResultDto> SaveStudentDetail(StudentFormViewModel student);
        Task<StudentProfileDto> GetStudentProfileAsync(int StudentID);
        Task UpdateStudentProfileAsync(EditStudentProfileDto model);
        Task<StudentFormViewModel> GetStudentForEditAsync(int studentId);
        Task<bool> UpdateStudentAsync(StudentFormViewModel model);
        Task<StudentFormViewModel> RebuildEditModelAsync(StudentFormViewModel model);
    }
}
