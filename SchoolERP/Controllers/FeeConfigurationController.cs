using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolERP.Controllers
{
    [Authorize(Roles = "Admin")] // 🔒 IMPORTANT
    public class FeeConfigurationController : Controller
    {
        private readonly IFeeConfigurationService _service;

        public FeeConfigurationController(IFeeConfigurationService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _service.GetAllAsync();
            return View(data);
        }

        public IActionResult Create()
        {
            var vm = new FeeConfigurationVM
            {
                FeeTypes = GetFeeTypes(),
                LateFineTypes = GetLateFineTypes()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(FeeConfigurationVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.FeeTypes = GetFeeTypes();
                vm.LateFineTypes = GetLateFineTypes();
                return View(vm);
            }

            var entity = new FeeConfiguration
            {
                AcademicYearID = vm.AcademicYearID,
                FeeType = vm.FeeType,
                DueDay = vm.DueDay,
                GraceDays = vm.GraceDays,
                LateFineType = vm.LateFineType,
                LateFineAmount = vm.LateFineAmount,
                MaxMonthsPerPayment = vm.MaxMonthsPerPayment,
                IsActive = vm.IsActive
            };

            await _service.CreateAsync(entity);
            return RedirectToAction("Index");
        }

        private List<SelectListItem> GetFeeTypes() => new()
        {
            new SelectListItem("Monthly", "Monthly"),
            new SelectListItem("Quarterly", "Quarterly")
        };

        private List<SelectListItem> GetLateFineTypes() => new()
        {
            new SelectListItem("Daily", "DAILY"),
            new SelectListItem("Monthly", "MONTHLY"),
            new SelectListItem("Slab", "SLAB")
        };


        // GET: Edit Partial
        public async Task<IActionResult> Edit(int id)
        {
            var data = await _service.GetByIdAsync(id);

            if (data == null) return NotFound();

            var vm = new FeeConfigurationVM
            {
                ConfigId = data.ConfigId,
                AcademicYearID = data.AcademicYearID,
                FeeType = data.FeeType,
                DueDay = data.DueDay,
                GraceDays = data.GraceDays,
                LateFineType = data.LateFineType,
                LateFineAmount = data.LateFineAmount,
                MaxMonthsPerPayment = data.MaxMonthsPerPayment,
                IsActive = data.IsActive
            };

            return PartialView("_EditFeeConfig", vm);
        }


        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit(FeeConfigurationVM vm)
        {
            if (!ModelState.IsValid)
                return PartialView("_EditFeeConfig", vm);

            var entity = await _service.GetByIdAsync(vm.ConfigId);

            entity.FeeType = vm.FeeType;
            entity.DueDay = vm.DueDay;
            entity.GraceDays = vm.GraceDays;
            entity.LateFineType = vm.LateFineType;
            entity.LateFineAmount = vm.LateFineAmount;
            entity.MaxMonthsPerPayment = vm.MaxMonthsPerPayment;
            entity.IsActive = vm.IsActive;

            await _service.UpdateAsync(entity);

            return Json(new { success = true });
        }



        // GET: Delete Partial
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _service.GetByIdAsync(id);
            return PartialView("_DeleteConfig", data);
        }


        // POST: Delete Confirm
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return Json(new { success = true });
        }

    }
}
