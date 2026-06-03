using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Enum;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;

namespace SchoolERP.Controllers
{
    [Authorize(Roles = "Admin, Teacher, Student")]
    public class HolidayController : Controller
    {
        private readonly IHolidayService _service;

        public HolidayController(IHolidayService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {

            var today = DateTime.Today;

            var holidays = await _service.GetAllAsync();

            var nextHoliday = holidays
                .Where(x => x.StartDate >= today)
                .OrderBy(x => x.StartDate)
                .FirstOrDefault();

            var vm = new HolidayDashboardVM
            {
                TotalHolidays = holidays.Count,

                NextHoliday = nextHoliday,

                UpcomingHolidays = holidays
                    .Count(x => x.StartDate >= today),

                VacationDays = holidays
                    .Where(x => x.Type == HolidayType.Vacation)
                    .Sum(x => (x.EndDate - x.StartDate).Days + 1),

                RecurringHolidays = holidays
                    .Count(x => x.IsRecurring),

                Holidays = holidays
            };

            return View(vm);
        }

        public async Task<IActionResult> Create(int? id)
        {
            if (id == null)
            {
                return View(new Holiday()); // CREATE mode
            }

            var data = await _service.GetByIdAsync(id.Value);

            if (data == null)
                return NotFound();

            return View(data); // EDIT mode
        }


        [HttpPost]
        public async Task<IActionResult> Create(Holiday model)
        {


            // UPDATE
            if (model.Id > 0)
            {
                var data = await _service.UpdateAsync(model);

                if (!data.Success)
                {
                    ModelState.AddModelError("", data.Message);
                    return View(model);
                }

                TempData["Success"] = "Holiday updated successfully";
                return RedirectToAction("Index");
            }

            // CREATE
            var result = await _service.CreateAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["Success"] = "Holiday created successfully"; ;
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction("Index");
        }

    }
}
