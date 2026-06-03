using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Models.DTOS;

namespace SchoolERP.Controllers
{
    [Authorize(Roles = "Admin, SuperAdmin, Teacher")]
    public class TeacherBirthdaysController : Controller
    {
        private readonly AppDbContext _context;

        public TeacherBirthdaysController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, int? month, string? filter)
        {
            ViewBag.Search = search;
            ViewBag.Month = month;
            var today = DateTime.Today;

            // =========================
            // BASE QUERY
            // =========================
            var query =
                from t in _context.Teachers
                join g in _context.Genders
                    on t.GenderID equals g.GenderID
                where t.IsActive
                      && t.DateOfBirth.HasValue

                select new TeacherBirthdayDTO
                {
                    TeacherId = t.TeacherID,
                    TeacherName = t.FullName,
                    Gender = g.GenderName,
                    Designation = t.Designation,
                    DOB = t.DateOfBirth.Value
                };

            // =========================
            // SEARCH
            // =========================
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x => x.TeacherName.Contains(search));
            }

            // =========================
            // MONTH FILTER
            // =========================
            if (month.HasValue)
            {
                query = query.Where(x =>
                    x.DOB.Month == month.Value);
            }
            
            // =========================
            // FETCH
            // =========================
            var data = await query
                .AsNoTracking()
                .OrderBy(x => x.TeacherName)
                .ToListAsync();

            if (!string.IsNullOrEmpty(filter))
            {
                switch (filter)
                {
                    case "Today":
                        data = data
                            .Where(x => x.DaysRemaining == 0)
                            .ToList();
                        break;

                    case "ThisWeek":
                        data = data
                            .Where(x => x.DaysRemaining <= 7)
                            .ToList();
                        break;

                    case "ThisMonth":
                        data = data
                            .Where(x => x.UpcomingDate.Month == today.Month)
                            .ToList();
                        break;
                }
            }

            // =========================
            // UPCOMING CALCULATION
            // =========================
            foreach (var item in data)
            {
                var nextBirthday = new DateTime(
                    today.Year,
                    item.DOB.Month,
                    item.DOB.Day);

                if (nextBirthday < today)
                {
                    nextBirthday = nextBirthday.AddYears(1);
                }

                item.UpcomingDate = nextBirthday;

                item.DaysRemaining =
                    (nextBirthday - today).Days;
            }

            // =========================
            // SORT
            // =========================
            var upcoming = data
                .OrderBy(x => x.DaysRemaining)
                .ThenBy(x => x.TeacherName)
                .ToList();

            return View(upcoming);
        }



    }
}
