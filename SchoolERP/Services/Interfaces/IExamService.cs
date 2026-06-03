using SchoolERP.Common;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;

namespace SchoolERP.Services.Interfaces
{
    public interface IExamService
    {
        Task<List<ExamVM>> GetAllAsync();
        Task<(bool Success, string Message)> CreateAsync(Exam exam);
        Task<(bool Success, string Message)> UpdateAsync(Exam exam);
        Task<Exam?> GetByIdAsync(int id);

        Task<ServiceResponse> CreateExamAsync(Exam exam);
        Task<ServiceResponse> SaveScheduleAsync(List<ExamSchedule> schedules);
        Task<ServiceResponse> SaveGradeRulesAsync(List<GradeRule> rules);
    }
}
