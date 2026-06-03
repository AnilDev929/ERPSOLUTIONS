using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using System.Security.Claims;

namespace SchoolERP.Controllers
{
    [Authorize]
    public class BirthdaysController : Controller
    {
        private readonly AppDbContext _context;

        public BirthdaysController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? classId)
        {
            var today = DateTime.Today;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var isTeacher = User.IsInRole("Teacher");

            // =========================
            // BASE QUERY
            // =========================
            var query =
                (from s in _context.Students
                 join se in _context.StudentEnrollments on s.StudentID equals se.StudentId
                 join c in _context.Classes on se.ClassId equals c.ClassId
                 join sec in _context.Sections on se.SectionId equals sec.SectionId
                 join g in _context.Genders on s.GenderID equals g.GenderID
                 where s.IsActive
                 select new UpcomingBirthdayDTO
                 {
                     StudentId = s.StudentID,
                     StudentName = s.StudentName,
                     Gender = g.GenderName.ToLower(),
                     ClassId = c.ClassId,
                     ClassName = c.ClassName + " - " + sec.SectionName,
                     DOB = s.DateOfBirth.Value
                 });


            // =========================
            // TEACHER FILTER
            // =========================
            if (isTeacher)
            {
                var teacherClassIds = await _context.ClassSections
                    .Where(x => x.ClassTeacherId == userId)
                    .Select(x => x.ClassId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(x => teacherClassIds.Contains(x.ClassId));
            }
            // =========================
            // ADMIN CLASS FILTER
            // =========================
            else if (classId.HasValue)
            {
                query = query.Where(x => x.ClassId == classId.Value);
            }

            // =========================
            // FETCH DATA
            // =========================
            var data = await query.ToListAsync();

            // =========================
            // UPCOMING DATE CALCULATION
            // =========================
            foreach (var item in data)
            {
                // Birthday in current year
                var nextBirthday = new DateTime(
                    today.Year,
                    item.DOB.Month,
                    item.DOB.Day);

                // If already passed this year
                if (nextBirthday < today)
                {
                    nextBirthday = nextBirthday.AddYears(1);
                }
                item.UpcomingDate = nextBirthday;
                item.DaysRemaining =
                    (nextBirthday - today).Days;
            }
            
            // =========================
            // SHOW ONLY TODAY + UPCOMING
            // NEXT 30 DAYS
            // =========================
            var upcoming = data
                // Only upcoming birthdays
                .Where(x => x.DaysRemaining >= 0
                    && x.DaysRemaining <= 30
                )
                .OrderBy(x => x.DaysRemaining)
                // Then alphabetical
                .ThenBy(x => x.StudentName)
                .ToList();
            
            // =========================
            // VIEWBAG
            // =========================
            ViewBag.ClassId = classId;
            // Admin dropdown only
            if (!isTeacher)
            {
                ViewBag.Classes = await _context.Classes
                    .OrderBy(x => x.ClassOrder)
                    .ToListAsync();
            }

            return View(upcoming);
        }
    
    
    
    }
}
