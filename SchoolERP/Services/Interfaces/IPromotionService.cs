using SchoolERP.Common;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;

namespace SchoolERP.Services.Interfaces
{
    public interface IPromotionService
    {
        Task<List<PromotionSettingDto>> GetPromotionSettingsAsync();
        Task<PromotionState> GetPromotionStateAsync(int academicYearId);
        Task<PromotionSetting> GetByAcademicYearAsync(int academicYearId);
        Task<PromotionSetting?> GetByIdAsync(int id);
        Task<ServiceResponse> ValidateAsync(PromotionSetting model);
        Task<ServiceResponse> SaveAsync(PromotionSetting model);
    }
}
