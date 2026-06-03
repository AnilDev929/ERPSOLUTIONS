using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Implementations;
using SchoolERP.Services.Interfaces;

namespace SchoolERP.Controllers
{
    public class GradeRuleController : Controller
    {
        private readonly IGradeRuleService _service;

        public GradeRuleController(IGradeRuleService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var data = await _service.GetAllAsync();
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? id)
        {
            if (!id.HasValue)
                return View(new GradeRule());

            var rule = new GradeRule();

            if (id.HasValue)
            {
                rule = await _service.GetByIdAsync(id.Value);

                if (rule == null)
                    return NotFound();
            }

            return View(rule); // no need to manually map
        }

        [HttpPost]
        public async Task<IActionResult> Create(GradeRule modeldata)
        {
            if (!ModelState.IsValid)
                return View(modeldata);

            // 🔥 Business Rule: Validate Range
            if (modeldata.MinScore > modeldata.MaxScore)
            {
                ModelState.AddModelError("", "Min score cannot be greater than Max score.");
                return View(modeldata);
            }

            // 🔥 Auto-set fail flag (optional but smart)
            if (modeldata.ResultStatus?.ToUpper() == "FAIL")
                modeldata.IsFail = true;

            // 🔥 Default timestamps (optional safety)
            if (modeldata.GradeRuleID == 0)
                modeldata.CreatedAt = DateTime.Now;

            var result = modeldata.GradeRuleID == 0
                ? await _service.CreateAsync(modeldata)
                : await _service.UpdateAsync(modeldata);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(modeldata);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted.Success)
            {
                TempData["Error"] = deleted.Message;
                return RedirectToAction("Index");
            }
            TempData["Success"] = deleted.Message;
            return RedirectToAction("Index");

        }



    }
}
