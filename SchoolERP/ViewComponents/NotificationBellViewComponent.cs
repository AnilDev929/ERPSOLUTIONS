using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Models.ViewModels;
using System.Security.Claims;

namespace SchoolERP.ViewComponents
{
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public NotificationBellViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userIdClaim = UserClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            int userId = string.IsNullOrEmpty(userIdClaim)
                ? 0
                : int.Parse(userIdClaim);
            int roleId = Convert.ToInt32(UserClaimsPrincipal.FindFirst("RoleID")?.Value);

            //var unreadCount = await _context.NotificationReadStatus
            //    .CountAsync(x => x.UserId == userId && !x.IsRead);

            var today = DateTime.Today;

            var count = await _context.Notifications
                .Where(n => n.IsActive
                    && n.Targets.Any(t => t.RoleId == roleId)
                    && n.EndDate >= today   // 🔥 include future + active
                    && !_context.NotificationReadStatus
                        .Any(r => r.NotificationId == n.NotificationId
                               && r.UserId == userId
                               && r.IsRead))
                .CountAsync();

            var topNotifications = await _context.Notifications
                .Where(n => n.IsActive 
                    && n.Targets.Any(t => t.RoleId == roleId) 
                    && n.EndDate >= today)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .Select(x => new NotificationItemVM
                {
                    NotificationId = x.NotificationId,
                    Title = x.Title
                })
                .ToListAsync();

            var model = new NotificationBellVM
            {
                UnreadCount = count,
                TopNotifications = topNotifications
            };

            return View(model);
        }

    }
}
