
using SchoolERP.Enum;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;

namespace SchoolERP.Services.Interfaces
{
    public interface ILeaveService
    {
        Task<List<LeaveType>> GetActiveLeaveTypesAsync();
        Task<List<LeaveHistoryDto>> GetUserLeavesAsync(int userId);
        Task<decimal> GetTotalApprovedLeavesAsync(int userId);
        //Task<bool> CanApplyLeaveAsync(int userId, DateTime fromDate, DateTime toDate, int? leaveId = null);
        Task<bool> CanApplyLeaveAsync(int userId, DateTime fromDate, DateTime toDate, bool isHalfDay = false, string halfDayType = null, int? leaveId = null);

        Task<bool> ApplyLeaveAsync(LeaveRequest request);
        Task<List<LeaveTypeViewModel>> LeaveTakenSummeryAsync(int employeeId);
        Task<List<LeaveApprovalDto>> GetLeavesPendingApprovalAsync();
        Task<bool> ApproveLeaveAsync(int leaveId, int principalId, LeaveStatus status);
        Task<bool> RejectLeaveAsync(int leaveId, int principalId, string Comment);
        Task<bool> CancelLeaveAsync(int leaveId, int userId);
    }
}
