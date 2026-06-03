using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;

namespace SchoolERP.Services.Interfaces
{
    public interface IAcademicYearService
    {
        IEnumerable<AcademicYearDropdownDto> GetAll();
        IEnumerable<AcademicYear> GetAllAsync();
        Task<AcademicYear?> GetByIdAsync(int id);
        Task<string> AddAsync(AcademicYear model);
        Task<string> UpdateAsync(AcademicYear model);
        Task DeleteAsync(int id);
        Task<AcademicYear> GetCurrentAcademicYearAsync();
    }
}
