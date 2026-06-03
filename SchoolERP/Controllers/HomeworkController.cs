using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using System.Security.Claims;

namespace SchoolERP.Controllers
{
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class HomeworkController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        private readonly AppDbContext _context;
        public HomeworkController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        protected int GetLoggedInTeacherId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim == null)
                throw new Exception("Sorry, you’re not authorized to access this page.");

            // convert to int if needed
            int userId = int.Parse(claim);

            // 🔐 Get logged-in teacher
            int? teacherId = _context.Teachers
                .Where(t => t.UserID == userId)
                .Select(t => t.TeacherID)
                .FirstOrDefault();

            return teacherId.Value;
        }

        // =====================================================
        // HOMEWORK DASHBOARD
        // =====================================================
        public async Task<IActionResult> Index()
        {
            int teacherId = GetLoggedInTeacherId();  // from login

            try
            {
                var model = new TeacherHomeworkDashboardVM
                {
                    TotalActive = await _context.Homework
                    .CountAsync(x =>
                        x.TeacherId == teacherId &&
                        x.Status == Enum.HomeworkStatus.Active),

                    PendingReview = await _context.HomeworkSubmission
                    .CountAsync(x =>
                        x.Status == Enum.SubmissionStatus.Submitted),

                    SubmittedToday = await _context.HomeworkSubmission
                    .CountAsync(x =>
                        x.SubmittedAt.HasValue &&
                        x.SubmittedAt.Value.Date == DateTime.Today),

                    Overdue = await _context.Homework
                    .CountAsync(x =>
                        x.DueDate < DateTime.Now &&
                        x.Status == Enum.HomeworkStatus.Active),

                    Homeworks = await _context.Homework
                    .Where(x => x.TeacherId == teacherId)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new TeacherHomeworkCardVM
                    {
                        HomeworkId = x.HomeworkId,

                        SubjectName = x.Subject.SubjectName,

                        ClassName = x.Class.ClassName,

                        Title = x.Title,

                        DueDate = x.DueDate,

                        Status = x.DueDate < DateTime.Now
                            ? "Overdue"
                            : "Active",

                        SubmittedCount = _context.HomeworkSubmission
                            .Count(s => s.HomeworkId == x.HomeworkId),

                        TotalStudents = _context.StudentEnrollments
                            .Count(s => s.ClassId == x.ClassId  && s.SectionId == x.SectionId)
                    })
                    .ToListAsync()
                };

                return View(model);
            }
            catch (Exception)
            {
                return View(new TeacherHomeworkDashboardVM());
            }
            
        }


        // =====================================================
        // CREATE HOMEWORK - GET
        // =====================================================
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new CreateHomeworkVM
            {
                AssignedDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(7),
                ClassList = _context.Classes
                    .Select(x => new SelectListItem
                    {
                        Value = x.ClassId.ToString(),
                        Text = x.ClassName
                    }),
            };
            ViewBag.IsEdit = false;
            return View(vm);
        }

        [HttpGet]
        public IActionResult GetSectionsByClass(int classId)
        {
            var teacherId = GetLoggedInTeacherId();

            var sections = _context.ClassSections
                .Where(x => x.ClassId == classId)
                .Select(x => new
                {
                    x.Section.SectionId,
                    x.Section.SectionName,
                    x.Id
                })
                .ToList();

            return Json(sections);
        }

        [HttpGet]
        public IActionResult GetSubjectsByClass(int classSectionId)
        {
            var teacherId = GetLoggedInTeacherId();

            var subjects = _context.ClassSectionSubject
                .Where(x => x.ClassSectionId == classSectionId)
                .Select(x => new
                {
                    x.Subject.SubjectId,
                    x.Subject.SubjectName
                })
                .ToList();

            return Json(subjects);
        }

        private async Task SaveAttachments(List<IFormFile> files, int homeworkId)
        {
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "homework");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            foreach (var file in files)
            {
                var uniqueFileName =
                    Guid.NewGuid() +
                    Path.GetExtension(file.FileName);

                var filePath = Path.Combine(
                    uploadPath,
                    uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var attachment = new HomeworkAttachment
                {
                    HomeworkId = homeworkId,

                    FileName = file.FileName,

                    FilePath ="/uploads/homework/" + uniqueFileName,

                    FileType = file.ContentType,

                    UploadedAt = DateTime.Now
                };

                _context.HomeworkAttachment.Add(attachment);
            }
        }

        private async Task LoadDropdowns(CreateHomeworkVM model)
        {
            model.ClassList = await _context.Classes
                .Select(x => new SelectListItem
                {
                    Value = x.ClassId.ToString(),
                    Text = x.ClassName
                }).ToListAsync();

            model.SectionList = await _context.ClassSections
                .Where(x => x.ClassId == model.ClassId)
                .Select(x => new SelectListItem
                {
                    Value = x.SectionId.ToString(),
                    Text = x.Section.SectionName
                }).ToListAsync();

            var classSection = await _context.ClassSections
                .FirstOrDefaultAsync(x =>
                    x.ClassId == model.ClassId &&
                    x.SectionId == model.SectionId);

            if (classSection != null)
            {
                model.SubjectList =
                    await _context.ClassSectionSubject
                    .Where(x => x.ClassSectionId == classSection.Id)
                    .Select(x => new SelectListItem
                    {
                        Value = x.SubjectId.ToString(),
                        Text = x.Subject.SubjectName
                    }).ToListAsync();
            }
        }

        // =====================================================
        // CREATE HOMEWORK - POST
        // =====================================================

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(CreateHomeworkVM model)
        //{
        //    // Due date validation
        //    if (model.DueDate < model.AssignedDate)
        //    {
        //        ModelState.AddModelError(
        //            "DueDate",
        //            "Due date cannot be earlier than assigned date.");
        //    }

        //    // File validation
        //    if (model.Files != null && model.Files.Any())
        //    {
        //        foreach (var file in model.Files)
        //        {
        //            var extension = Path.GetExtension(file.FileName)
        //                .ToLower();

        //            var allowedExtensions = new[]
        //            {
        //                ".pdf",
        //                ".doc",
        //                ".docx",
        //                ".jpg",
        //                ".jpeg",
        //                ".png"
        //            };

        //            if (!allowedExtensions.Contains(extension))
        //            {
        //                ModelState.AddModelError(
        //                    "",
        //                    "Only PDF, DOC, DOCX, JPG, PNG files are allowed.");
        //            }

        //            if (file.Length > (10 * 1024 * 1024))
        //            {
        //                ModelState.AddModelError(
        //                    "",
        //                    "File size cannot exceed 10MB.");
        //            }
        //        }
        //    }

        //    // Reload dropdowns if validation fails
        //    if (!ModelState.IsValid)
        //    {
        //        model.ClassList = _context.Classes
        //            .Select(x => new SelectListItem
        //            {
        //                Value = x.ClassId.ToString(),
        //                Text = x.ClassName
        //            });

        //        return View(model);
        //    }

        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // Logged-in Teacher
        //        var teacherId = GetLoggedInTeacherId();

        //        // Save Homework
        //        var homework = new Homework
        //        {
        //            ClassId = model.ClassId,
        //            SectionId = model.SectionId,
        //            SubjectId = model.SubjectId,
        //            TeacherId = teacherId,
        //            Title = model.Title,
        //            Description = model.Description,
        //            Instructions = model.Instructions,
        //            AssignedDate = model.AssignedDate,
        //            DueDate = model.DueDate,
        //            TotalMarks = model.TotalMarks,
        //            Priority = model.Priority,
        //            Status = Enum.HomeworkStatus.Active,
        //            HomeworkType = model.HomeworkType,
        //            AllowLateSubmission = model.AllowLateSubmission,
        //            CreatedAt = DateTime.Now
        //        };

        //        _context.Homework.Add(homework);
        //        await _context.SaveChangesAsync();

        //        // =====================================================
        //        // FILE UPLOAD
        //        // =====================================================
        //        if (model.Files != null && model.Files.Any())
        //        {
        //            var uploadPath = Path.Combine(
        //                _environment.WebRootPath,
        //                "uploads",
        //                "homework");

        //            if (!Directory.Exists(uploadPath))
        //            {
        //                Directory.CreateDirectory(uploadPath);
        //            }

        //            foreach (var file in model.Files)
        //            {
        //                var uniqueFileName =
        //                    Guid.NewGuid().ToString() +
        //                    Path.GetExtension(file.FileName);

        //                var filePath = Path.Combine(
        //                    uploadPath,
        //                    uniqueFileName);

        //                using (var stream = new FileStream(
        //                    filePath,
        //                    FileMode.Create))
        //                {
        //                    await file.CopyToAsync(stream);
        //                }

        //                var attachment = new HomeworkAttachment
        //                {
        //                    HomeworkId = homework.HomeworkId,
        //                    FileName = file.FileName,
        //                    FilePath = "/uploads/homework/" + uniqueFileName,
        //                    FileType = file.ContentType,
        //                    UploadedAt = DateTime.Now
        //                };

        //                _context.HomeworkAttachment.Add(attachment);
        //            }

        //            await _context.SaveChangesAsync();
        //            await transaction.CommitAsync();
        //        }

        //        TempData["success"] = "Homework created successfully.";

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        await transaction.RollbackAsync();
        //        Console.WriteLine(ex.InnerException?.Message);
        //        return View(model);
        //    }
        //    catch (Exception)
        //    {
        //        await transaction.RollbackAsync();
        //        return View(model);
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateHomeworkVM model)
        {
            // =====================================================
            // VALIDATIONS
            // =====================================================
            if (model.DueDate < model.AssignedDate)
            {
                ModelState.AddModelError(
                    "DueDate",
                    "Due date cannot be earlier than assigned date.");
            }

            // File Validation
            if (model.Files != null && model.Files.Any())
            {
                foreach (var file in model.Files)
                {
                    var extension = Path.GetExtension(file.FileName)
                        .ToLower();

                    var allowedExtensions = new[]
                    {
                        ".pdf",
                        ".doc",
                        ".docx",
                        ".jpg",
                        ".jpeg",
                        ".png"
                    };

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Only PDF, DOC, DOCX, JPG, PNG files are allowed.");
                    }

                    // 10 MB
                    if (file.Length > (10 * 1024 * 1024))
                    {
                        ModelState.AddModelError("", "File size cannot exceed 10MB.");
                    }
                }
            }

            // =====================================================
            // RELOAD DROPDOWNS
            // =====================================================

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model);
                return View(model);
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                Homework homework;

                // =====================================================
                // CREATE OR EDIT
                // =====================================================

                if (model.HomeworkId.HasValue)
                {
                    // EDIT
                    homework = await _context.Homework
                        .Include(x => x.HomeworkAttachments)
                        .FirstOrDefaultAsync(x =>
                            x.HomeworkId == model.HomeworkId.Value);

                    if (homework == null)
                    {
                        return NotFound();
                    }

                    // Update Fields
                    homework.ClassId = model.ClassId;
                    homework.SectionId = model.SectionId;
                    homework.SubjectId = model.SubjectId;

                    homework.Title = model.Title;
                    homework.Description = model.Description;
                    homework.Instructions = model.Instructions;

                    homework.AssignedDate = model.AssignedDate;
                    homework.DueDate = model.DueDate;

                    homework.TotalMarks = model.TotalMarks;
                    homework.Priority = model.Priority;

                    homework.HomeworkType = model.HomeworkType;
                    homework.AllowLateSubmission = model.AllowLateSubmission;
                    homework.UpdatedAt = DateTime.Now;

                    // =====================================================
                    // REPLACE OLD FILES
                    // =====================================================

                    if (model.Files != null && model.Files.Any())
                    {
                        // DELETE OLD FILES FROM FOLDER
                        foreach (var oldFile in homework.HomeworkAttachments)
                        {
                            if (!string.IsNullOrEmpty(oldFile.FilePath))
                            {
                                var oldPath = Path.Combine(
                                    _environment.WebRootPath,
                                    oldFile.FilePath.TrimStart('/')
                                );

                                if (System.IO.File.Exists(oldPath))
                                {
                                    System.IO.File.Delete(oldPath);
                                }
                            }
                        }

                        // DELETE OLD DB RECORDS
                        _context.HomeworkAttachment.RemoveRange(
                            homework.HomeworkAttachments);

                        // SAVE NEW FILES
                        await SaveAttachments(
                            model.Files,
                            homework.HomeworkId);
                    }
                }
                else
                {
                    // CREATE

                    var teacherId = GetLoggedInTeacherId();

                    homework = new Homework
                    {
                        ClassId = model.ClassId,
                        SectionId = model.SectionId,
                        SubjectId = model.SubjectId,

                        TeacherId = teacherId,

                        Title = model.Title,
                        Description = model.Description,
                        Instructions = model.Instructions,

                        AssignedDate = model.AssignedDate,
                        DueDate = model.DueDate,

                        TotalMarks = model.TotalMarks,
                        Priority = model.Priority,
                        HomeworkType = model.HomeworkType,
                        AllowLateSubmission =model.AllowLateSubmission,
                        Status = Enum.HomeworkStatus.Active,
                        CreatedAt = DateTime.Now
                    };

                    _context.Homework.Add(homework);

                    await _context.SaveChangesAsync();

                    // =====================================================
                    // SAVE ATTACHMENTS
                    // =====================================================

                    if (model.Files != null && model.Files.Any())
                    {
                        await SaveAttachments(
                            model.Files,
                            homework.HomeworkId);
                    }
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["success"] =
                    model.HomeworkId.HasValue
                    ? "Homework updated successfully."
                    : "New Homework created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Console.WriteLine(ex.Message);

                await LoadDropdowns(model);

                return View(model);
            }
        }

        // =====================================================
        // HOMEWORK DETAILS
        // =====================================================

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var homework = await _context.Homework
                   .Include(x => x.Subject)
                   .Include(x => x.Class)
                   .Include(x => x.Section)
                   .Include(x => x.Teacher)
                   .Include(x => x.HomeworkAttachments)
                   .FirstOrDefaultAsync(x => x.HomeworkId == id);

                if (homework == null)
                {
                    return NotFound();
                }

                var vm = new HomeworkDetailsViewModel
                {
                    HomeworkId = homework.HomeworkId,
                    Title = homework.Title,
                    Description = homework.Description,
                    Instructions = homework.Instructions,

                    SubjectName = homework.Subject?.SubjectName,
                    ClassName = homework.Class?.ClassName,
                    SectionName = homework.Section?.SectionName,
                    TeacherName = homework.Teacher?.FullName,

                    AssignedDate = homework.AssignedDate,
                    DueDate = homework.DueDate,

                    TotalMarks = homework.TotalMarks,

                    Priority = homework.Priority.ToString(),
                    Status = homework.Status.ToString(),
                    HomeworkType = homework.HomeworkType.ToString(),

                    AllowLateSubmission = homework.AllowLateSubmission,

                    Attachments = homework.HomeworkAttachments?
                                .Select(a => new HomeworkAttachment
                                {
                                    HomeworkAttachmentId = a.HomeworkAttachmentId,
                                    FileName = a.FileName,
                                    FilePath = a.FilePath,
                                    FileType = Path.GetExtension(a.FileName)
                                }).ToList()
                };

                return View(vm);
            }
            catch (Exception)
            {
                return View(new HomeworkDetailsViewModel());
            }
        }

        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var file = await _context.HomeworkAttachment
                .FirstOrDefaultAsync(x => x.HomeworkAttachmentId == id);

            if (file == null)
                return NotFound();

            // Remove starting slash if exists
            var relativePath = file.FilePath.TrimStart('/');

            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("File not found on server.");
            }

            var memory = new MemoryStream();

            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;

            return File(memory, "application/octet-stream", file.FileName);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var homework = _context.Homework
            .Include(x => x.HomeworkAttachments)
            .FirstOrDefault(x => x.HomeworkId == id);

            if (homework == null)
                return NotFound();

            // FIND CLASS SECTION
            var classSection = await _context.ClassSections
                .FirstOrDefaultAsync(x =>
                    x.ClassId == homework.ClassId &&
                    x.SectionId == homework.SectionId);

            try
            {

                var vm = new CreateHomeworkVM
                {
                    HomeworkId = homework.HomeworkId,

                    ClassId = homework.ClassId,
                    SectionId = homework.SectionId,
                    SubjectId = homework.SubjectId,

                    Instructions = homework.Instructions,
                    AssignedDate = homework.AssignedDate,
                    DueDate = homework.DueDate,

                    TotalMarks = homework.TotalMarks,
                    Priority = homework.Priority,

                    AllowLateSubmission = homework.AllowLateSubmission,

                    Title = homework.Title,
                    Description = homework.Description,

                    ClassList = _context.Classes
                        .Select(x => new SelectListItem
                        {
                            Value = x.ClassId.ToString(),
                            Text = x.ClassName
                        }).ToList(),

                    // PRELOAD SECTION
                    SectionList = _context.ClassSections
                        .Where(x => x.ClassId == homework.ClassId)
                        .Select(x => new SelectListItem
                        {
                            Value = x.Section.SectionId.ToString(),
                            Text = x.Section.SectionName
                        }).ToList(),

                    // PRELOAD SUBJECT
                    SubjectList = classSection != null
                    ? _context.ClassSectionSubject
                        .Where(x => x.ClassSectionId == classSection.Id)
                        .Select(x => new SelectListItem
                        {
                            Value = x.Subject.SubjectId.ToString(),
                            Text = x.Subject.SubjectName
                        }).ToList()
                    : new List<SelectListItem>(),

                    // EXISTING ATTACHMENTS
                    ExistingAttachments = homework.HomeworkAttachments?
                        .Select(a => new HomeworkAttachment
                        {
                            HomeworkAttachmentId = a.HomeworkAttachmentId,
                            FileName = a.FileName
                        }).ToList()
                        ?? new List<HomeworkAttachment>()

                };

                ViewBag.IsEdit = true;
                return View("Create", vm);
            }
            catch (Exception)
            {
                ViewBag.IsEdit = true;
                return View("Create", new CreateHomeworkVM());
            }

        }


        // =====================================================
        // DELETE HOMEWORK
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var homework = await _context.Homework
                .FirstOrDefaultAsync(x => x.HomeworkId == id);

            if (homework == null)
            {
                return NotFound();
            }

            homework.Status = Enum.HomeworkStatus.Archived;
            homework.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["success"] = "Homework archived successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // REVIEW SUBMISSIONS
        // =====================================================

        public async Task<IActionResult> Review(int id)
        {
            var submissions = await _context.HomeworkSubmission
                .Include(x => x.Student)
                .Where(x => x.HomeworkId == id)
                .ToListAsync();

            ViewBag.HomeworkId = id;

            return View(submissions);
        }













    }
}
