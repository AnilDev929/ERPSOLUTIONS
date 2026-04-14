using ERP_SOLUTIONS.Models.Entities;
using ERP_SOLUTIONS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ERP_SOLUTIONS.Controllers
{
    public class ClassesController : Controller
    {
        private readonly IClassService _classService;

        public ClassesController(IClassService classService)
        {
            _classService = classService;
        }

        public async Task<IActionResult> Index()
        {
            var classes = await _classService.GetAllClassesAsync();
            return View(classes);
        }

        // GET: Create
        public IActionResult Create()
        {
            ViewBag.Title = "Add Class";
            return View(new ClassModel());
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassModel model)
        {
            if (!ModelState.IsValid) return View(model);

            bool success = await _classService.AddClassAsync(model);
            if (success)
            {
                TempData["Success"] = $"Class '{model.ClassName}' added successfully!";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, "Duplicate class name found.");
            return View(model);
        }


        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var cls = await _classService.GetClassByIdAsync(id);
            if (cls == null) return NotFound();

            ViewBag.Title = "Edit Class";
            return View("Create", cls); // reuse Create view
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClassModel model)
        {
            if (id != model.ClassId) return BadRequest();
            if (!ModelState.IsValid) return View("Create", model);

            bool success = await _classService.UpdateClassAsync(model);
            if (success)
            {
                TempData["Success"] = $"Class '{model.ClassName}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, "Duplicate class name found.");
            return View("Create", model);
        }

        // GET: Delete confirmation
        public async Task<IActionResult> Delete(int id)
        {
            var cls = await _classService.GetClassByIdAsync(id);
            if (cls == null) return NotFound();

            return View(cls);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool success = await _classService.DeleteClassAsync(id);
            if (success)
            {
                TempData["Success"] = "Class deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "Error deleting class!";
            return RedirectToAction(nameof(Index));
        }

    }
}
