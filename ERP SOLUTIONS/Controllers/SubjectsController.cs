using ERP_SOLUTIONS.Models.Entities;
using ERP_SOLUTIONS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ERP_SOLUTIONS.Controllers
{
    public class SubjectsController : Controller
    {
        private readonly ISubjectService _subjectService;
        private readonly ILogger<SubjectsController> _logger;

        public SubjectsController(ISubjectService subjectService, ILogger<SubjectsController> logger)
        {
            _subjectService = subjectService;
            _logger = logger;
        }

        // GET: Subjects
        public async Task<IActionResult> Index()
        {
            var subjects = await _subjectService.GetAllSubjectsAsync();
            return View(subjects);
        }

        // GET: Subjects/Create
        public IActionResult Create()
        {
            // Always pass a new Subject model to the View
            return View(new Subject());
        }

        // POST: Subjects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subject subject)
        {
            if (!ModelState.IsValid)
                return View(subject);

            var success = await _subjectService.AddSubjectAsync(subject);

            if (success)
            {
                TempData["Success"] = $"Subject '{subject.SubjectName}' added successfully!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Add duplicate error to ModelState
                ModelState.AddModelError(string.Empty, "Duplicate subject name or code found.");
                return View(subject); // same request, ModelState passes to view
            }
        }

        // GET: Subjects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest();

            var subject = await _subjectService.GetSubjectByIdAsync(id.Value);
            if (subject == null) return NotFound();

            ViewBag.Title = "Edit Subject"; // Change title dynamically
            return View("Create", subject);  // Reuse Create.cshtml
        }

        // POST: Subjects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Subject subject)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Title = "Edit Subject";
                return View("Create", subject);
            }

            var success = await _subjectService.UpdateSubjectAsync(subject);

            if (success)
            {
                TempData["Success"] = $"Subject '{subject.SubjectName}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Duplicate subject name or code found.");
                ViewBag.Title = "Edit Subject";
                return View("Create", subject);
            }
        }

        // GET: Subjects/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _subjectService.GetSubjectByIdAsync(id);
            if (subject == null) return NotFound();
            return View(subject);
        }

        // POST: Subjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _subjectService.DeleteSubjectAsync(id);
            if (success)
            {
                TempData["Success"] = "Subject deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete subject.";
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
