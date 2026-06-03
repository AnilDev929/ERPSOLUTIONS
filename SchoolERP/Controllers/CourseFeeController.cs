using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace SchoolERP.Controllers
{
    [Authorize(Roles = "Admin")] // 🔒 IMPORTANT
    public class CourseFeeController : Controller
    {
        private readonly ICourseFeeService _service;

        public CourseFeeController(ICourseFeeService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int? classId, int? yearId)
        {
            return Json(await _service.GetAllAsync(classId, yearId));
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            return Json(await _service.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Save(CourseFee model)
        {
            var result = await _service.SaveAsync(model);
            return Json(new
            {
                success = result.success,
                message = result.message
            });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetDropdowns()
        {
            return Json(await _service.GetDropdownsAsync());
        }
    }
}
