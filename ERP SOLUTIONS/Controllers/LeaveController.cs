using ERP_SOLUTIONS.Enum;
using ERP_SOLUTIONS.Models.DTOS;
using ERP_SOLUTIONS.Models.Entities;
using ERP_SOLUTIONS.Models.ViewModels;
using ERP_SOLUTIONS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ERP_SOLUTIONS.Controllers
{
    public class LeaveController : Controller
    {
        private readonly ILeaveService _leaveService;
        

        public LeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        private async Task PopulateApplyPageData(LeaveApplyViewModel model, int userId)
        {
            var leaveTypes = await _leaveService.GetActiveLeaveTypesAsync();
            ViewBag.LeaveTypes = new SelectList(leaveTypes, "Id", "Name");

            model.LeaveHistory = await _leaveService.GetUserLeavesAsync(userId);
            model.leaveSummery = await _leaveService.LeaveTakenSummeryAsync(userId);
            model.TotalLeaves = model.LeaveHistory.Count();
        }

        // Teacher: Apply Leave
        // GET: Apply leave page
        [HttpGet]
        //[Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Apply()
        {
            // Get logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ✅ 1. Get Leave Types (for dropdown)
            var leaveTypes = await _leaveService.GetActiveLeaveTypesAsync();

            ViewBag.LeaveTypes = new SelectList(leaveTypes, "Id", "Name");

            // ✅ 2. Get Leave History for user
            var leaves = await _leaveService.GetUserLeavesAsync(Convert.ToInt32(userId));
            var leaveSummery = await _leaveService.LeaveTakenSummeryAsync(Convert.ToInt32(userId));
            
            //Total Leave Applied
            var leaveCount = leaves.Count();

            // Count Approved Leaves
            var approvedCount = await _leaveService.GetTotalApprovedLeavesAsync(Convert.ToInt32(userId));
            // Count Cancelled Leaves
            var cancelledCount = leaves.Count(l => l.Status == ERP_SOLUTIONS.Enum.LeaveStatus.Cancelled);
            ViewBag.ApprovedLeaves = approvedCount;
            ViewBag.CancelledLeaves = cancelledCount;

            // ✅ 4. Prepare ViewModel
            var vm = new LeaveApplyViewModel
            {
                leaveSummery = leaveSummery,
                LeaveHistory = leaves,
                TotalLeaves = leaveCount,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now
            };

            return View(vm);
        }


        // POST: Submit leave
        [HttpPost]
        //[Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Apply(LeaveApplyViewModel model)
        {
            try
            {
                // Manual validation logic
                if (model.LeaveTypeId == 0)
                {
                    ModelState.AddModelError("LeaveTypeId", "Please select leave type");
                }

                if (model.FromDate == default)
                {
                    ModelState.AddModelError("FromDate", "From Date is required");
                }

                if (model.ToDate == default)
                {
                    ModelState.AddModelError("ToDate", "To Date is required");
                }

                if (model.ToDate < model.FromDate)
                {
                    ModelState.AddModelError("ToDate", "To Date cannot be earlier than From Date");
                }

                if (model.IsHalfDay && string.IsNullOrEmpty(model.HalfDayType))
                {
                    ModelState.AddModelError("HalfDayType", "Please select half day type");
                }

                if (string.IsNullOrWhiteSpace(model.Reason))
                {
                    ModelState.AddModelError("Reason", "Reason is required");
                }




                // Get logged-in user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Check overlapping leave
                //bool canApply = await _leaveService.CanApplyLeaveAsync(Convert.ToInt32(userId), model.FromDate, model.ToDate);
                bool canApply = await _leaveService.CanApplyLeaveAsync(Convert.ToInt32(userId), model.FromDate, model.ToDate,
                    model.IsHalfDay, model.HalfDayType);

                if (!canApply)
                {
                    TempData["Error"] = "You already have a leave (Pending or Approved) in the selected date range.";
                    var leaveTypes = await _leaveService.GetActiveLeaveTypesAsync();
                    ViewBag.LeaveTypes = new SelectList(leaveTypes, "Id", "Name");
                    model.LeaveHistory = await _leaveService.GetUserLeavesAsync(Convert.ToInt32(userId));
                    model.TotalLeaves = await _leaveService.GetTotalApprovedLeavesAsync(Convert.ToInt32(userId));
                    model.FromDate = DateTime.Now;
                    model.ToDate = DateTime.Now;

                    return View(model);
                }

                if (ModelState.IsValid)
                {
                    //Half - Day Validation(Backend)
                    if (model.IsHalfDay && model.FromDate != model.ToDate)
                    {
                        TempData["Error"] = "Half-day must be a single date.";
                        var leaveTypes = await _leaveService.GetActiveLeaveTypesAsync();
                        ViewBag.LeaveTypes = new SelectList(leaveTypes, "Id", "Name");
                        return View(model);
                    }

                    // 2️⃣ Create leave request
                    var leaveRequest = new LeaveRequest
                    {
                        UserId = Convert.ToInt32(userId),
                        LeaveTypeId = model.LeaveTypeId,
                        FromDate = model.FromDate,
                        ToDate = model.ToDate,
                        IsHalfDay = model.IsHalfDay,
                        HalfDayType = model.HalfDayType,
                        TotalDays = CalculateTotalDays(model.FromDate, model.ToDate, model.IsHalfDay),
                        Reason = model.Reason,
                        Status = LeaveStatus.Pending
                    };

                    bool flag = await _leaveService.ApplyLeaveAsync(leaveRequest);
                    if (!flag)
                    {
                        TempData["Error"] = "Failed to apply leave.";
                        return View(model);
                    }
                    TempData["Success"] = "Leave applied successfully!";
                    return RedirectToAction("Apply");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.InnerException?.Message ?? ex.Message;
                return View(model);
            }
        }


        /// <summary>
        /// Calculates total leave days based on from/to dates and half-day selection.
        /// </summary>
        /// <param name="fromDate">Start date of leave</param>
        /// <param name="toDate">End date of leave</param>
        /// <param name="isHalfDay">Whether the leave is half-day</param>
        /// <returns>Total leave days as decimal</returns>
        public decimal CalculateTotalDays(DateTime fromDate, DateTime toDate, bool isHalfDay = false)
        {
            if (toDate < fromDate)
                throw new ArgumentException("End date cannot be before start date.");

            // Calculate the total number of days (inclusive)
            decimal totalDays = (toDate - fromDate).Days + 1;

            // If half-day, reduce 0.5
            if (isHalfDay)
                totalDays -= 0.5m;

            return totalDays;
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            //var leave = _context.Leaves.FirstOrDefault(l => l.Id == id);

            //if (leave == null)
            //    return NotFound();

            //// Prevent cancelling approved leave
            //if (leave.Status == "Approved")
            //{
            //    TempData["Error"] = "Approved leave cannot be cancelled.";
            //    return RedirectToAction("Apply");
            //}

            //// Option 1: Mark as Cancelled
            //leave.Status = "Cancelled";

            //// Option 2 (alternative): Delete
            //// _context.Leaves.Remove(leave);

            //_context.SaveChanges();

            TempData["Success"] = "Leave cancelled successfully.";
            return RedirectToAction("Apply");
        }

        // GET: List all leaves
        // Admin: Manage Leave
        //[Authorize(Roles = "Admin")]
        //ApprovalList
        public async Task<IActionResult> ApprovalList()
        {
            try
            {
                var pendingLeaves = await _leaveService.GetLeavesPendingApprovalAsync();
                return View(pendingLeaves);
            }
            catch(Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(new List<LeaveApprovalDto>());
            }
           
        }

        // POST: Approve/Reject leave (admin/teacher action)
        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> TakeAction(int id, LeaveStatus status)
        {
            int leaveId = id;
            // Get logged-in user
            var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool flag = false;
            if(status == LeaveStatus.Approved)
            {
                flag = await _leaveService.ApproveLeaveAsync(leaveId, userId, status);
            }
            else if (status == LeaveStatus.Rejected)
            {
                flag = await _leaveService.RejectLeaveAsync(leaveId, userId);
            }
            else if (status == LeaveStatus.Cancelled)
            {
                flag = await _leaveService.CancelLeaveAsync(leaveId, userId);
                TempData["Success"] = "Leave cancelled successfully.";
                return RedirectToAction("Apply");
            }


            //var success = await _adminLeaveService.TakeActionAsync(leaveId, status);
            return RedirectToAction("ApprovalList");
        }
    }
}
