using Microsoft.AspNetCore.Mvc;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;
using System.Security.Claims;

namespace SchoolERP.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        //Get the Admin/SuperAdmin LoginID 
        protected int GetLoggedInUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim == null)
                throw new Exception("Sorry, you’re not authorized to access this page.");

            // convert to int if needed
            int userId = int.Parse(claim);
            return userId;
        }

        // CREATE PAGE
        public async Task<IActionResult> Create(int? id)
        {
            var model = await _service.GetByIdForEditAsync(id.HasValue ? id.Value : 0);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(NotificationVM model)
        {
            // 🔴 VALIDATION RULES
            if (string.IsNullOrWhiteSpace(model.Title))
                ModelState.AddModelError("Title", "Title is required");

            if (string.IsNullOrWhiteSpace(model.Message))
                ModelState.AddModelError("Message", "Message is required");

            if (model.StartDate < DateTime.Now)
                ModelState.AddModelError("StartDate", "Past date not allowed");

            if (model.StartDate >= model.EndDate)
                ModelState.AddModelError("", "End time must be greater than start time");

            if (model.RoleIds == null || !model.RoleIds.Any())
                ModelState.AddModelError("", "Select at least one role");

            //if (!ModelState.IsValid)
            //    return View(model);

            var userid = GetLoggedInUserId();
            var result = await _service.CreateOrUpdateAsync(model, userid);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }

        // ADMIN LIST
        public async Task<IActionResult> Index()
        {
            var data = await _service.GetNotifications();

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.SoftDeleteAsync(id);

            if (result == null)
                return Json(new { success = false });

            return Json(new { success = result.Success });
        }
    }
}
