using ERP_SOLUTIONS.Models.DTOS;
using ERP_SOLUTIONS.Models.Entities;
using ERP_SOLUTIONS.Models.ViewModels;

namespace ERP_SOLUTIONS.Services.Interfaces
{
    public interface IStudentProfileService
    {
        Task<CreateStudentResultDto> SaveStudentDetail(StudentFormViewModel student);
        Task<Student> GetStudentProfileAsync(int StudentID);
        Task UpdateStudentProfileAsync(Student model);
    }
}
