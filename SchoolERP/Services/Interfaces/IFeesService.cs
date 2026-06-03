using SchoolERP.Models.ViewModels;

namespace SchoolERP.Services.Interfaces
{
    public interface IFeesService
    {
        Task<StudentFeeDetailViewModel> GetStudentFeesAsync(int studentId);
        Task<FeePaymentViewModel> ReadFeePaymentAsync(int userId, int month);
        Task<FeePaymentViewModel> CalculateFeesAmount(int userId, int month);
        Task<bool> ProcessPaymentAsync(FeePaymentViewModel model);
    }
}
