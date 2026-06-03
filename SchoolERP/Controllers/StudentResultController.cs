using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;

namespace SchoolERP.Controllers
{
    public class StudentResultController : Controller
    {
        private readonly AppDbContext _context;

        public StudentResultController(AppDbContext db)
        {
            _context = db;
        }

        public IActionResult MarksEntry(int studentId)
        {
            ViewBag.Exams = _context.Exams.ToList();
            return View();
        }

        public IActionResult Result(int studentId, int examId)
        {
            ViewBag.Exams = new SelectList(_context.Exams, "ExamId", "ExamName");

            var result = _context.StudentMarks
                .Where(x => x.StudentId == studentId && x.ExamId == examId)
                .Select(x => new ResultDTO
                {
                    SubjectName = x.Subject.SubjectName,
                    Marks = x.MarksObtained.Value,
                    MaxMarks = x.MaxMarks,
                    //Status = x.MarksObtained >= x.PassMarks ? "Pass" : "Fail"
                })
                .ToList();

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveMarks(MarksEntryViewModel model)
        {
            if (model == null)
                return BadRequest();

            if (model.Students == null || !model.Students.Any())
            {
                ModelState.AddModelError("", "No student data found.");
                return View(model);
            }

            // Load grade rules from DB (DO NOT trust client)
            var gradeRules = _context.GradeRules.ToList();

            for (int i = 0; i < model.Students.Count; i++)
            {
                var s = model.Students[i];

                if (s.StudentId <= 0)
                {
                    ModelState.AddModelError($"Students[{i}].StudentId", "Invalid student.");
                    continue;
                }

                if (s.IsAbsent)
                {
                    // If absent → marks must be null or 0
                    s.MarksObtained = null;
                    s.Grade = "ABSENT";
                }
                else
                {
                    if (!s.MarksObtained.HasValue)
                    {
                        ModelState.AddModelError($"Students[{i}].MarksObtained", "Marks required.");
                        continue;
                    }

                    if (s.MarksObtained < 0 || s.MarksObtained > 100)
                    {
                        ModelState.AddModelError($"Students[{i}].MarksObtained", "Marks must be between 0 and 100.");
                        continue;
                    }

                    // Recalculate grade on server (IMPORTANT)
                    var rule = gradeRules
                        .FirstOrDefault(r => s.MarksObtained >= r.MinScore && s.MarksObtained <= r.MaxScore);

                    s.Grade = rule?.GradeLetter ?? "N/A";
                }
            }

            if (!ModelState.IsValid)
                return View(model);

            // Save (use transaction in production)
            foreach (var s in model.Students)
            {
                var existing = _context.StudentMarks.FirstOrDefault(x =>
                    x.StudentId == s.StudentId &&
                    x.ClassId == model.ClassId &&
                    x.SectionId == model.SectionId &&
                    x.SubjectId == model.SubjectId &&
                    x.ExamId == model.ExamId);

                if (existing != null)
                {
                    existing.MarksObtained = s.MarksObtained;
                }
                else
                {
                    var entity = new StudentMarks
                    {
                        StudentId = s.StudentId,
                        ExamId = model.ExamId.Value,
                        SubjectId = model.SubjectId.Value,
                        MarksObtained = s.MarksObtained,
                        Grade = s.Grade,
                        IsAbsent = s.IsAbsent,
                        Remarks = s.Remark
                    };

                    _context.StudentMarks.Add(entity);
                }
            }

            _context.SaveChanges();

            TempData["Success"] = "Marks saved successfully!";

            return RedirectToAction("MarksEntry");
        }
    }
}
