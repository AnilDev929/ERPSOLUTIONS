using SchoolERP.Common;
using SchoolERP.Models.Entities;

namespace SchoolERP.Services.Interfaces
{
    public interface IGradeRuleService
    {
        Task<List<GradeRule>> GetAllAsync();
        Task<GradeRule?> GetByIdAsync(int id);
        Task<ServiceResponse> CreateAsync(GradeRule rule);
        Task<ServiceResponse> UpdateAsync(GradeRule rule);
        Task<ServiceResponse> DeleteAsync(int id);
    }
}
