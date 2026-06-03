using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;

namespace SchoolERP.Controllers
{
    public class ExamController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IExamService _service;

        public ExamController(IExamService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        private async Task LoadYears(ExamVM vm)
        {
            vm.AcademicYears = await _context.AcademicYears
                .Select(x => new SelectListItem
                {
                    Value = x.AcademicYearID.ToString(),
                    Text = x.YearName
                }).ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await _service.GetAllAsync();
                return View(data);
            }
            catch (Exception)
            {
                // log ex if logger available
                TempData["Error"] = "Unable to load exams. Please try again.";
                return View(new List<ExamVM>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? id)
        {
            var vm = new ExamVM();

            var today = DateTime.Today;
            var currentYear = _context.AcademicYears
                .FirstOrDefault(x => x.YearStart <= today && x.YearEnd >= today);
            vm.AcademicYearID = currentYear?.AcademicYearID ?? 0;

            if (id.HasValue)
            {
                var exam = await _service.GetByIdAsync(id.Value);
                if (exam != null)
                {
                    vm.ExamId = exam.ExamId;
                    vm.ExamName = exam.ExamName;
                    vm.Description = exam.Description;
                    vm.AcademicYearID = exam.AcademicYearID;
                    vm.IsActive = exam.IsActive;
                    vm.EndDate = exam.EndDate;
                    vm.StartDate = exam.StartDate;
                    vm.ExamType = exam.ExamType;
                }
            }

            await LoadYears(vm);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ExamVM vm)
        {
            if (!ModelState.IsValid)
            {
                //// Loop through all ModelState entries
                //foreach (var entry in ModelState)
                //{
                //    var key = entry.Key; // The field name
                //    foreach (var error in entry.Value.Errors)
                //    {
                //        // This prints to console (for debugging)
                //        Console.WriteLine($"Field: {key}, Error: {error.ErrorMessage}");
                //    }
                //}

                var today = DateTime.Today;
                var currentYear = _context.AcademicYears
                    .FirstOrDefault(x => x.YearStart <= today && x.YearEnd >= today);
                vm.AcademicYearID = currentYear?.AcademicYearID ?? 0;

                await LoadYears(vm);
                return View(vm);
            }

            // 🔥 1. Basic validation
            if (vm.StartDate == null || vm.EndDate == null)
            {
                ModelState.AddModelError("StartDate", "Start Date and End Date are required.");
                return View(vm);
            }
            // 🔥 2. Date range validation
            if (vm.StartDate > vm.EndDate)
            {
                ModelState.AddModelError("EndDate", "Start Date cannot be greater than End Date.");
                return View(vm);
            }

            // 🔥 3. Optional: Prevent past end date
            if (vm.EndDate < DateTime.Today)
            {
                ModelState.AddModelError("Date", "End Date cannot be in the past.");
                return View(vm);
            }
            
            // 🔥 4. Optional: Overlapping exam check (same academic year)
            //var overlap = _context.Exams.Any(x =>
            //    x.AcademicYearID == vm.AcademicYearID &&
            //    x.ExamId != vm.ExamId &&
            //    (
            //        (vm.StartDate >= x.StartDate && vm.StartDate <= x.EndDate) ||
            //        (vm.EndDate >= x.StartDate && vm.EndDate <= x.EndDate)
            //    )
            //);

            var overlap = _context.Exams.Any(x =>
                x.AcademicYearID == vm.AcademicYearID &&
                x.ExamId != vm.ExamId &&
                vm.StartDate <= x.EndDate &&
                vm.EndDate >= x.StartDate
            );

            if (overlap)
            {
                ModelState.AddModelError("", "Another exam already exists in this date range.");
                return View(vm);
            }

            var exam = new Exam
            {
                ExamId = vm.ExamId,
                ExamName = vm.ExamName,
                Description = vm.Description,
                AcademicYearID = vm.AcademicYearID,
                ExamType = vm.ExamType,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                IsActive = vm.IsActive
            };

            var result = vm.ExamId == 0
                ? await _service.CreateAsync(exam)
                : await _service.UpdateAsync(exam);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                await LoadYears(vm);
                return RedirectToAction("Index");
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }


    }
}
