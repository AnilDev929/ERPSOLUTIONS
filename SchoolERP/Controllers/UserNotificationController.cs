using Microsoft.AspNetCore.Mvc;
using SchoolERP.Services.Interfaces;
using System.Security.Claims;

namespace SchoolERP.Controllers
{
    public class UserNotificationController : Controller
    {
        private readonly INotificationService _service;

        public UserNotificationController(INotificationService service)
        {
            _service = service;
        }

        protected int GetLoggedInUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim == null)
                throw new Exception("Sorry, you’re not authorized to access this page.");

            // convert to int if needed
            int userId = int.Parse(claim);
            return userId;
        }

        public async Task<IActionResult> Inbox()
        {
            int userId = GetLoggedInUserId();  // from login

            int roleId = Convert.ToInt32(User.FindFirst("RoleID")?.Value);
            var data = await _service.GetActiveForUserAsync(userId, roleId);

            return View(data);
        }

        public async Task<IActionResult> Read(int id)
        {
            int userId = GetLoggedInUserId();
            await _service.MarkAsReadAsync(id, userId);
            return RedirectToAction("Inbox");
        }
    }
}
