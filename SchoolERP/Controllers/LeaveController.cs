using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolERP.Enum;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SchoolERP.Controllers
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
            var cancelledCount = leaves.Count(l => l.Status == LeaveStatus.Cancelled);
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

                if (model.ToDate < model.FromDate)
                {
                    ModelState.AddModelError("", "To Date cannot be earlier than From Date");
                }
                else
                {
                    int totalDays = (model.ToDate.Date - model.FromDate.Date).Days + 1;
                    model.DaysTaken = totalDays;
                }

                if (model.IsHalfDay && string.IsNullOrEmpty(model.HalfDayType))
                {
                    ModelState.AddModelError("HalfDayType", "Please select half day type");
                }

                if (string.IsNullOrWhiteSpace(model.Reason))
                {
                    ModelState.AddModelError("Reason", "Please provide leave Reason.");
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

                //foreach (var state in ModelState)
                //{
                //    string fieldName = state.Key;

                //    foreach (var error in state.Value.Errors)
                //    {
                //        string errorMessage = error.ErrorMessage;
                //        Exception exception = error.Exception;

                //        Console.WriteLine($"Field: {fieldName}");
                //        Console.WriteLine($"Error: {errorMessage}");
                //    }
                //}

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

                    string? filePath = null;
                    // Upload File
                    if (model.LeaveDocument != null && model.LeaveDocument.Length > 0)
                    {
                        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                        var extension = Path.GetExtension(model.LeaveDocument.FileName).ToLower();
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("LeaveDocument", "Only PDF, DOC, DOCX, JPG, PNG files are allowed.");
                        }

                        // 5 MB
                        if (model.LeaveDocument.FileName.Length > (5 * 1024 * 1024))
                        {
                            ModelState.AddModelError("LeaveDocument", "File size cannot exceed 10MB.");
                        }
                        // Folder path
                        string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(),
                                                           "wwwroot/uploads/leave-documents");

                        // Create folder if not exists
                        if (!Directory.Exists(uploadFolder))
                        {
                            Directory.CreateDirectory(uploadFolder);
                        }

                        // Unique file name
                        //string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.LeaveDocument.FileName;
                        //string uniqueFileName = $"{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{extension}";

                        string originalName = Path.GetFileNameWithoutExtension(model.LeaveDocument.FileName);
                        string safeName = Regex.Replace(originalName, @"[^a-zA-Z0-9_-]", "");
                        string uniqueFileName = $"{userId}_" +
                            $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_" +
                            $"{safeName:N}{extension}";

                        string fullPath = Path.Combine(uploadFolder, uniqueFileName);

                        // Save file
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await model.LeaveDocument.CopyToAsync(stream);
                        }

                        // Save relative path to DB
                        filePath = "/uploads/leave-documents/" + uniqueFileName;
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
                        Status = LeaveStatus.Pending,
                        DocumentPath = filePath
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
        public async Task<IActionResult> TakeAction(int id, LeaveStatus status, string? comment)
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
                flag = await _leaveService.RejectLeaveAsync(leaveId, userId, comment);
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
