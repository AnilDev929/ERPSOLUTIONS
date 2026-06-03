using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using System.Security.Claims;

namespace SchoolERP.Controllers
{
    public class StudentHomeworkController : Controller
    {

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StudentHomeworkController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _env = environment;
        }

        protected int GetLoggedInStudentId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim == null)
                throw new Exception("Sorry, you’re not authorized to access this page.");

            // convert to int if needed
            int userId = int.Parse(claim);

            // 🔐 Get logged-in teacher
            int studentID = _context.Students
                .Where(s => s.UserID == userId)
                .Select(t => t.StudentID)
                .FirstOrDefault();

            return studentID;
        }

        // =====================================================
        // HOMEWORK LIST (STUDENT DASHBOARD)
        // =====================================================

        public async Task<IActionResult> Index()
        {
            int studentId = GetLoggedInStudentId(); // replace with logged-in user

            var studentClassId = await _context.StudentEnrollments
                .Where(s => s.StudentId == studentId)
                .Select(s => s.ClassId)
                .FirstOrDefaultAsync();


            var submissions = _context.HomeworkSubmission
    .Where(s => s.StudentId == studentId)
    .Select(s => s.HomeworkId);

            var data = await _context.Homework
                .Where(h => h.ClassId == studentClassId)
                .Select(h => new StudentHomeworkVM
                {
                    HomeworkId = h.HomeworkId,
                    SubjectName = h.Subject.SubjectName,
                    Title = h.Title,
                    Description = h.Description,
                    DueDate = h.DueDate,

                    IsSubmitted = submissions.Contains(h.HomeworkId),

                    Status = submissions.Contains(h.HomeworkId)
                        ? "Submitted"
                        : (h.DueDate < DateTime.Now ? "Overdue" : "Pending"),

                    DaysLeft = EF.Functions.DateDiffDay(DateTime.Now, h.DueDate)
                })
                .ToListAsync();



            return View(data);
        }


        // =====================================================
        // DETAILS PAGE
        // =====================================================
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var homework = await _context.Homework
                .Include(h => h.Subject)
                .FirstOrDefaultAsync(h => h.HomeworkId == id);

                if (homework == null)
                    return NotFound();

                return View(homework);
            }
            catch (Exception)
            {
                return View(new Homework());
            }

        }

        // =====================================================
        // SUBMIT PAGE (GET)
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Submit(int id)
        {
            var homework = await _context.Homework
                .Include(h => h.Subject)
                .FirstOrDefaultAsync(h => h.HomeworkId == id);

            if (homework == null)
                return NotFound();

            var vm = new SubmitHomeworkVM
            {
                HomeworkId = homework.HomeworkId,
                Title = homework.Title,
                SubjectName = homework.Subject.SubjectName,
                DueDate = homework.DueDate
            };

            return View(vm);
        }

        // =====================================================
        // SUBMIT PAGE (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SubmitHomeworkVM model)
        {
            int studentId = GetLoggedInStudentId();

            if (!ModelState.IsValid)
                return View(model);

            // FILE VALIDATION
            if (model.File != null)
            {
                var allowed = new[] { ".pdf", ".doc", ".docx", ".jpg", ".png" };

                var ext = Path.GetExtension(model.File.FileName).ToLower();

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Invalid file type");
                    return View(model);
                }

                if (model.File.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "File size must be less than 10MB");
                    return View(model);
                }
            }

            // CHECK DUPLICATE SUBMISSION
            var alreadySubmitted = await _context.HomeworkSubmission
                .AnyAsync(x =>
                    x.HomeworkId == model.HomeworkId &&
                    x.StudentId == studentId);

            if (alreadySubmitted)
            {
                ModelState.AddModelError("", "Homework already submitted");
                return View(model);
            }

            // SAVE FILE
            string filePath = null;

            if (model.File != null)
            {
                var folder = Path.Combine(_env.WebRootPath, "uploads/homework");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.File.FileName);

                var fullPath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                filePath = "/uploads/homework/" + fileName;
            }

            // SAVE SUBMISSION
            var submission = new HomeworkSubmission
            {
                HomeworkId = model.HomeworkId,
                StudentId = studentId,
                SubmissionText = model.Remarks,
                AttachmentPath = filePath,
                SubmittedAt = DateTime.Now,
                Status = Enum.SubmissionStatus.Submitted // Submitted
            };

            _context.HomeworkSubmission.Add(submission);

            await _context.SaveChangesAsync();

            TempData["success"] = "Homework submitted successfully";

            return RedirectToAction("Index");
        }


    }
}
