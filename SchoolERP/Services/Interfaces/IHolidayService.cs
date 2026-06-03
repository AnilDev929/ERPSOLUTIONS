using SchoolERP.Common;
using SchoolERP.Models.Entities;

namespace SchoolERP.Services.Interfaces
{
    public interface IHolidayService
    {
        Task<List<Holiday>> GetAllAsync();
        Task<Holiday> GetByIdAsync(int id);
        Task<ServiceResponse> CreateAsync(Holiday model);
        Task<ServiceResponse> UpdateAsync(Holiday model);
        Task<ServiceResponse> DeleteAsync(int id);
    }
}
