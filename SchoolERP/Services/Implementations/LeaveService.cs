using SchoolERP.Data;
using SchoolERP.Enum;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SchoolERP.Services.Implementations
{
    public class LeaveService : ILeaveService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LeaveService> _logger;
        public LeaveService(AppDbContext context, ILogger<LeaveService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ Get active leave types for dropdown
        public async Task<List<LeaveType>> GetActiveLeaveTypesAsync()
        {
            try
            {
                return await _context.LeaveTypes
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leave types");
                return new List<LeaveType>();
            }
        }

        // ✅ Total approved leaves for a user
        public async Task<decimal> GetTotalApprovedLeavesAsync(int userId)
        {
            try
            {
                return await _context.LeaveRequests
                    .Where(l => l.UserId == userId && l.Status == LeaveStatus.Approved)
                    .SumAsync(l => l.TotalDays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total approved leaves for user {UserId}", userId);
                return 0;
            }
        }

        public async Task<List<LeaveTypeViewModel>> LeaveTakenSummeryAsync(int employeeId)
        {
            try
            {
                var leaveData = await _context.LeaveTypes
                .Where(lt => lt.IsActive)
                .Select(lt => new LeaveTypeViewModel
                {
                    LeaveTypeName = lt.Name,
                    MaxDaysAllowed = lt.MaxDaysPerYear,

                    DaysTaken = _context.LeaveRequests
                        .Where(lr => lr.UserId == employeeId
                                  && lr.LeaveTypeId == lt.Id
                                  && lr.Status == LeaveStatus.Approved) // important!
                        .Sum(lr => (int?)lr.TotalDays) ?? 0
                })
                .OrderBy(x => x.LeaveTypeName)
                .ToListAsync();

                return leaveData;
            }
            catch(Exception)
            {
                return (new List<LeaveTypeViewModel>());
            }
        }


        public async Task<List<LeaveHistoryDto>> GetUserLeavesAsync(int userId)
        {
            try
            {
                var data = await (from l in _context.LeaveRequests
                                  join t in _context.LeaveTypes
                                  on l.LeaveTypeId equals t.Id
                                  where l.UserId == userId
                                  select new LeaveHistoryDto
                                  {
                                      Id = l.Id,
                                      LeaveType = t.Name,
                                      FromDate = l.FromDate,
                                      ToDate = l.ToDate,
                                      TotalDays = l.TotalDays,
                                      Status = l.Status,
                                      Reason = l.Reason ?? "",
                                      DocumentPath = l.DocumentPath ?? "",
                                      Remark = l.Remarks ?? ""
                                  }).ToListAsync();

                return data;
            }
            catch (Exception ex)
            {
                // Log error (IMPORTANT)
                _logger.LogError(ex, "Error fetching leave history for user {UserId}", userId);

                // Option 1: return empty list (safe UI)
                return new List<LeaveHistoryDto>();

                // Option 2 (better for debugging):
                // throw;
            }

        }

        public async Task<bool> CanApplyLeaveAsync(int userId, DateTime fromDate, DateTime toDate
            , bool isHalfDay = false, string halfDayType = null, int? leaveId = null)
        //public async Task<bool> CanApplyLeaveAsync(int userId, DateTime fromDate, DateTime toDate, int? leaveId = null)
        {
            try
            {
                //// Fetch any leave that overlaps with requested dates
                //var overlappingLeave = await _context.LeaveRequests
                //    .Where(l => l.UserId == userId
                //                && (l.Status == "Approved" || l.Status == "Pending")
                //                && (l.FromDate <= toDate && l.ToDate >= fromDate) // overlap check
                //                && (!leaveId.HasValue || l.Id != leaveId.Value)) // ignore self if editing
                //    .AnyAsync();

                //return !overlappingLeave; // true if can apply, false if overlap exists

                var overlappingLeave = await _context.LeaveRequests
                .Where(l => l.UserId == userId
                            && (l.Status == LeaveStatus.Approved || l.Status == LeaveStatus.Pending)
                            && (!leaveId.HasValue || l.Id != leaveId.Value))
                .ToListAsync();

                foreach (var leave in overlappingLeave)
                {
                    // Full day overlap
                    if (!isHalfDay && !leave.IsHalfDay)
                    {
                        if (leave.FromDate <= toDate && leave.ToDate >= fromDate)
                            return false;
                    }

                    // Half-day overlap on same date
                    if (isHalfDay || leave.IsHalfDay)
                    {
                        // If dates match
                        var overlapDate = leave.IsHalfDay ? leave.FromDate : leave.FromDate;
                        if (overlapDate >= fromDate && overlapDate <= toDate)
                        {
                            // Both half-days on same date
                            if ((leave.IsHalfDay && isHalfDay && leave.FromDate == fromDate) ||
                                (!leave.IsHalfDay && isHalfDay && leave.FromDate == fromDate) ||
                                (leave.IsHalfDay && !isHalfDay && leave.FromDate == fromDate))
                            {
                                return false;
                            }
                        }
                    }
                }

                return true; // No overlap found
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking overlapping leave for user {UserId}", userId);
                return false; // fail-safe: prevent leave application if error occurs
            }
        }

        public async Task<bool> ApplyLeaveAsync(LeaveRequest request)
        {
            // 3. OPTIONAL: Soft balance check (DO NOT BLOCK)
            var balance = _context.LeaveBalances
               .FirstOrDefault(x => x.UserId == request.UserId
                                 && x.LeaveTypeId == request.LeaveTypeId
                                 && x.Year == DateTime.Now.Year);

            if (balance != null)
            {
                decimal available = balance.AllocatedDays
                                  + balance.CarriedForwardDays
                                  - balance.UsedDays;

                if (request.TotalDays > available)
                {
                    // ⚠️ Warning only, not rejection
                    // You can log or return special message
                    return true; // ("Leave applied, but may exceed available balance (subject to approval)");
                }
            }

            // 4. Save request (always allow)
            request.Status = LeaveStatus.Pending;

            _context.LeaveRequests.Add(request);
            await _context.SaveChangesAsync();

            return true; //"Leave applied successfully"
        }

        public async Task<bool> ApproveLeaveAsync(int leaveId, int adminID, LeaveStatus status)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var leave = await _context.LeaveRequests.FindAsync(leaveId);

            // Only allow Pending leaves to be acted upon
            if (leave == null || leave.Status != LeaveStatus.Pending)
                return false; //"Leave request not found"

            if (leave.Status != LeaveStatus.Pending)
                return false; // "Leave is already processed");

            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(x =>
                x.UserId == leave.UserId &&
                x.LeaveTypeId == leave.LeaveTypeId &&
                x.Year == DateTime.Now.Year);

            // ✅ Create balance if not exists
            if (balance == null)
            {
                var leaveType = await _context.LeaveTypes
                    .FirstOrDefaultAsync(x => x.Id == leave.LeaveTypeId);

                if (leaveType == null)
                    return false; //"Invalid leave type"

                balance = new LeaveBalance
                {
                    UserId = leave.UserId,
                    LeaveTypeId = leave.LeaveTypeId,
                    Year = DateTime.Now.Year,
                    AllocatedDays = leaveType.MaxDaysPerYear,
                    UsedDays = 0,
                    CarriedForwardDays = 0
                };

                _context.LeaveBalances.Add(balance);
            }

            if (status == LeaveStatus.Approved)
            {
                decimal available = balance.AllocatedDays
                                  + balance.CarriedForwardDays
                                  - balance.UsedDays;

                if (leave.TotalDays > available)
                    return false; // "Insufficient leave balance"

                balance.UsedDays += leave.TotalDays;
            }

            leave.Status = status;
            leave.ApprovedBy = adminID;
            leave.ApprovedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true; //"Leave processed successfully"
        }

        public async Task<bool> RejectLeaveAsync(int leaveId, int principalId, string Comment)
        {
            var leave = await _context.LeaveRequests.FindAsync(leaveId);

            if (leave == null) return false;

            leave.Status = LeaveStatus.Rejected;
            leave.ApprovedBy = principalId;
            leave.ApprovedDate = DateTime.Now;
            leave.Remarks = Comment;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelLeaveAsync(int leaveId, int userId)
        {
            try
            {
                var leave = await _context.LeaveRequests.FindAsync(leaveId);

                if (leave == null || leave.UserId != userId || leave.Status == LeaveStatus.Approved)
                    return false;

                leave.Status = LeaveStatus.Cancelled;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leaves pending approval");
                return false;
            }
        }

        public async Task<List<LeaveApprovalDto>> GetLeavesPendingApprovalAsync()
        {
            try
            {
                // Fetch all leaves that are Pending (awaiting admin/principal approval)
                var query = from l in _context.LeaveRequests
                            join t in _context.Teachers
                                on l.UserId equals t.UserID
                            join lt in _context.LeaveTypes
                                on l.LeaveTypeId equals lt.Id
                            where l.Status == LeaveStatus.Pending
                            orderby l.FromDate
                            select new LeaveApprovalDto
                            {
                                Id = l.Id,
                                TeacherName = t.FullName,
                                LeaveType = lt.Name,
                                FromDate = l.FromDate,
                                ToDate = l.ToDate,
                                TotalDays = l.TotalDays,
                                Reason = l.Reason ?? "",
                                Status = l.Status,
                                DocumentPath = l.DocumentPath ?? ""
                            };
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leaves pending approval");
                return new List<LeaveApprovalDto>();
            }
        }

    }
}
