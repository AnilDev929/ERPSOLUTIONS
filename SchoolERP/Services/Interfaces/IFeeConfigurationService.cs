using SchoolERP.Models.Entities;

namespace SchoolERP.Services.Interfaces
{
    public interface IFeeConfigurationService
    {
        Task<List<FeeConfiguration>> GetAllAsync();
        Task<FeeConfiguration> GetByIdAsync(int id);
        Task CreateAsync(FeeConfiguration entity);
        Task UpdateAsync(FeeConfiguration entity);
        Task DeleteAsync(int id);
    }
}
