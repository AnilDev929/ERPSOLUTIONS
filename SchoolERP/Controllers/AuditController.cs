using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Models.ViewModels;

namespace SchoolERP.Controllers
{
    public class AuditController : Controller
    {
        private readonly AppDbContext _context;

        public AuditController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // LIST PAGE
        // =========================
        public async Task<IActionResult> Logs(AuditLogFilterVM filter)
        {
            var query = _context.AuditLogs.AsQueryable();

            // SEARCH FILTER
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(x =>
                    x.UserName.Contains(filter.Search) ||
                    x.Action.Contains(filter.Search) ||
                    x.Controller.Contains(filter.Search));
            }

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(x => x.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.LogLevel))
                query = query.Where(x => x.LogLevel == filter.LogLevel);

            if (filter.FromDate.HasValue)
                query = query.Where(x => x.CreatedOn >= filter.FromDate);

            if (filter.ToDate.HasValue)
                query = query.Where(x => x.CreatedOn <= filter.ToDate);

            // TOTAL COUNT (IMPORTANT)
            filter.TotalRecords = await query.CountAsync();

            filter.PageNumber = filter.TotalRecords > 0 ? 1 : 0;
            
            // SAFETY: prevent invalid page numbers
            if (filter.PageNumber < 1)
                filter.PageNumber = 1;

            if (filter.PageSize <= 0)
                filter.PageSize = 10;

            if (filter.PageNumber > filter.TotalPages && filter.TotalPages > 0)
                filter.PageNumber = filter.TotalPages;

            // PAGINATION LOGIC
            filter.Logs = await query
                .OrderByDescending(x => x.CreatedOn)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return View(filter);
        }

        // =========================
        // DETAIL API (MODAL)
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetLogDetail(long id)
        {
            var log = await _context.AuditLogs
                .FirstOrDefaultAsync(x => x.AuditLogId == id);

            if (log == null)
                return NotFound();

            return Json(log);
        }
    }
}
