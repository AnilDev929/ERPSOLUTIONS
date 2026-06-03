using Microsoft.AspNetCore.Mvc;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;

namespace SchoolERP.Controllers
{
    public class PromotionControlController : Controller
    {
        private readonly IPromotionService _service;
        private readonly IAcademicYearService _yearService;

        public PromotionControlController(
            IPromotionService service,
            IAcademicYearService yearService)
        {
            _service = service;
            _yearService = yearService;
        }

        public async Task<IActionResult> Index()
        {
            var year = await _yearService.GetCurrentAcademicYearAsync();
            var model = await _service.GetByAcademicYearAsync(year.AcademicYearID)
                        ?? new PromotionSetting
                        {
                            AcademicYearId = year.AcademicYearID,
                            PromotionStartDate = DateTime.Today,
                            PromotionEndDate = DateTime.Today.AddDays(7)
                        };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(PromotionSetting model)
        {
            try
            {
                var result = await _service.SaveAsync(model);

                if (result.Success)
                    TempData["Success"] = result.Message;
                else
                    TempData["Error"] = result.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetPromotionSettings()
        {
            try
            {
                var data = await _service.GetPromotionSettingsAsync();
                return Json(data);
            }
            catch
            {
                return StatusCode(500, "Error loading promotion settings.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var data = await _service.GetByIdAsync(id);
                return Json(data);
            }
            catch(Exception)
            {
                return StatusCode(500, "Error loading promotion settings.");
            }
        }
    }
}
