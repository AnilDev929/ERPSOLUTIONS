using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;
using System.Security.Claims;

namespace SchoolERP.Controllers
{
    public class MarksController : Controller
    {
        private readonly IMarksService _service;

        private readonly AppDbContext _db;

        public MarksController(IMarksService service, AppDbContext db)
        {
            _service = service;
            _db = db;
        }

        protected int GetLoggedInUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim == null)
                throw new Exception("Sorry, you’re not authorized to access this page.");

            // convert to int if needed
            int userId = int.Parse(claim);
            return userId;
        }

        public IActionResult MarksEntry()
        {
            var vm = new MarksEntryVM
            {
                Classes = new SelectList(_db.Classes, "ClassId", "ClassName"),
                Exams = new SelectList(_db.Exams, "ExamId", "ExamName"),
            };

            return View(vm);
        }

        // AJAX: Load Sections by Class
        public async Task<IActionResult> GetSectionsByClass(int classId)
        {
            var sections = (from cs in _db.ClassSections
                            join s in _db.Sections
                                on cs.SectionId equals s.SectionId
                            where cs.ClassId == classId
                            select new
                            {
                                id = s.SectionId,
                                name = s.SectionName
                            }).ToList();

            return Json(sections);
        }

        // AJAX: Load Subjects by Class
        public JsonResult GetSubjectsByClass(int classId)
        {
            var subjects = (from cs in _db.ClassSections
                            join css in _db.ClassSectionSubject
                                on cs.Id equals css.ClassSectionId
                            join sub in _db.Subjects
                                on css.SubjectId equals sub.SubjectId
                            where cs.ClassId == classId
                                  && sub.IsActive
                            select new ClassSubjectDto
                            {
                                SubjectId = sub.SubjectId,
                                SubjectName = sub.SubjectName,
                                SubjectCode = sub.SubjectCode
                            })
                    .GroupBy(x => x.SubjectId)
                    .Select(g => g.First())
                    .ToList();

            return Json(subjects);
        }

        public JsonResult GetStudents(int classId, int sectionId)
        {
            var data = _service.GetStudents(classId, sectionId);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetGradeRules()
        {
            var rules = await _db.GradeRules
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.MinScore) // important
                .Select(x => new
                {
                    min = x.MinScore,
                    max = x.MaxScore,
                    grade = x.GradeLetter,
                    color = x.ColorCode
                })
                .ToListAsync();

            return Json(rules);
        }



        [HttpPost]
        public IActionResult Save(MarksEntryVM vm)
        {
            if (!ModelState.IsValid)
                return View("MarksEntry", vm);

            _service.SaveMarks(vm);

            TempData["Success"] = "Student Marks saved successfully";
            return RedirectToAction("MarksEntry");
        }
    }
}
