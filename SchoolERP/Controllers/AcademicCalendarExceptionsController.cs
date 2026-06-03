using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SchoolERP.Controllers
{
    [Authorize(Roles = "Admin")] // 🔒 IMPORTANT
    public class AcademicCalendarExceptionsController : Controller
    {
        private readonly AppDbContext _context;

        public AcademicCalendarExceptionsController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ INDEX
        public IActionResult Index(int? academicYearId, string exceptionType, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.AcademicCalendarExceptions
                .Include(x => x.AcademicYear)
                .AsQueryable();

            if (academicYearId.HasValue)
                query = query.Where(x => x.AcademicYearId == academicYearId);

            if (!string.IsNullOrEmpty(exceptionType))
                query = query.Where(x => x.ExceptionType == exceptionType);

            if (startDate.HasValue)
                query = query.Where(x => x.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.EndDate <= endDate.Value);

            var data = query.Select(x => new AcademicCalendarExceptionListVM
            {
                ExceptionId = x.ExceptionId,
                AcademicYearName = x.AcademicYear.YearName,
                ExceptionType = x.ExceptionType,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                IsBillingBlocked = x.IsBillingBlocked,
                Description = x.Description
            }).ToList();

            ViewBag.AcademicYears = _context.AcademicYears
                .Select(x => new SelectListItem
                {
                    Value = x.AcademicYearID.ToString(),
                    Text = x.YearName
                }).ToList();

            return View(data);
        }

        // ✅ CREATE (GET)
        public IActionResult Create()
        {
            var vm = new AcademicCalendarExceptionVM();
            LoadDropdowns(vm);
            return View("Upsert", vm);
        }

        // ✅ EDIT (GET)
        public IActionResult Edit(int id)
        {
            var entity = _context.AcademicCalendarExceptions.Find(id);

            if (entity == null)
                return NotFound();

            var vm = new AcademicCalendarExceptionVM
            {
                ExceptionId = entity.ExceptionId,
                AcademicYearId = entity.AcademicYearId,
                ExceptionType = entity.ExceptionType,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                IsBillingBlocked = entity.IsBillingBlocked,
                Description = entity.Description
            };

            LoadDropdowns(vm);
            return View("Upsert", vm);
        }

        // ✅ SAVE (CREATE + EDIT)
        [HttpPost]
        public IActionResult Save(AcademicCalendarExceptionVM vm)
        {
            // 🔹 Fetch academic year
            var year = _context.AcademicYears.Find(vm.AcademicYearId);

            if (year == null)
            {
                ModelState.AddModelError("", "Invalid academic year.");
            }

            // 🔹 Validate date range
            if (vm.StartDate > vm.EndDate)
            {
                ModelState.AddModelError("", "Start date cannot be after end date.");
            }

            if (year != null &&
                (vm.StartDate < year.YearStart || vm.EndDate > year.YearEnd))
            {
                ModelState.AddModelError("", "Dates must be within academic year.");
            }

            // 🔹 Overlap check
            var overlap = _context.AcademicCalendarExceptions.Any(x =>
                x.AcademicYearId == vm.AcademicYearId &&
                (vm.ExceptionId == null || x.ExceptionId != vm.ExceptionId) &&
                vm.StartDate <= x.EndDate &&
                vm.EndDate >= x.StartDate);

            if (overlap)
            {
                ModelState.AddModelError("", "Date range overlaps with existing exception.");
            }

            // 🔴 STOP if invalid
            if (!ModelState.IsValid)
            {
                LoadDropdowns(vm);
                return View("Upsert", vm);
            }

            // 🔹 SAVE
            if (vm.ExceptionId == null)
            {
                _context.AcademicCalendarExceptions.Add(new AcademicCalendarExceptions
                {
                    AcademicYearId = vm.AcademicYearId,
                    ExceptionType = vm.ExceptionType,
                    StartDate = vm.StartDate.Value,
                    EndDate = vm.EndDate.Value,
                    IsBillingBlocked = vm.IsBillingBlocked,
                    Description = vm.Description
                });
            }
            else
            {
                var entity = _context.AcademicCalendarExceptions.Find(vm.ExceptionId);

                if (entity == null)
                    return NotFound();

                entity.AcademicYearId = vm.AcademicYearId;
                entity.ExceptionType = vm.ExceptionType;
                entity.StartDate = vm.StartDate.Value;
                entity.EndDate = vm.EndDate.Value;
                entity.IsBillingBlocked = vm.IsBillingBlocked;
                entity.Description = vm.Description;
            }

            _context.SaveChanges();
            //return RedirectToAction("Index");

            return RedirectToAction("Index", new
            {
                academicYearId = vm.AcademicYearId,
                exceptionType = vm.ExceptionType,
                startDate = vm.StartDate.Value,
                endDate = vm.EndDate.Value
            });
        }

        // ✅ AJAX VALIDATION
        [HttpPost]
        public JsonResult ValidateDates(int academicYearId, DateTime startDate, DateTime endDate, int? exceptionId)
        {
            var year = _context.AcademicYears.Find(academicYearId);

            if (year == null)
            {
                return Json(new { isValid = false, message = "Invalid academic year." });
            }

            if (startDate > endDate)
            {
                return Json(new { isValid = false, message = "Start date cannot be after end date." });
            }

            if (startDate < year.YearStart || endDate > year.YearEnd)
            {
                return Json(new { isValid = false, message = "Dates must be within academic year." });
            }

            var overlap = _context.AcademicCalendarExceptions.Any(x =>
                x.AcademicYearId == academicYearId &&
                (exceptionId == null || x.ExceptionId != exceptionId) &&
                startDate <= x.EndDate &&
                endDate >= x.StartDate
            );

            if (overlap)
            {
                return Json(new { isValid = false, message = "Date range overlaps with existing record." });
            }

            return Json(new { isValid = true });
        }

        // ✅ DROPDOWN
        private void LoadDropdowns(AcademicCalendarExceptionVM vm)
        {
            var currentYearId = _context.AcademicYears
           .Where(y => y.IsActive) // or logic like DateTime.Now between start/end
           .Select(y => y.AcademicYearID)
           .FirstOrDefault();

            vm.AcademicYearId = currentYearId; // ✅ VERY IMPORTANT

            vm.AcademicYears = _context.AcademicYears
                .Select(y => new SelectListItem
                {
                    Value = y.AcademicYearID.ToString(),
                    Text = y.YearName
                }).ToList();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var entity = _context.AcademicCalendarExceptions
                .FirstOrDefault(x => x.ExceptionId == id);

            if (entity == null)
                return NotFound();

            _context.AcademicCalendarExceptions.Remove(entity);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
