using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;

namespace SchoolERP.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClassTeacherController : Controller
    {

        private readonly AppDbContext _context;

        public ClassTeacherController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // HELPER
        // =========================================================
        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            var words = name.Split(' ',
                StringSplitOptions.RemoveEmptyEntries);

            return string.Concat(words.Select(x => x[0]))
                .ToUpper();
        }

        //Replacement of AssignTeacherToClass for better Performance
        [HttpGet]
        public async Task<IActionResult> Index(int? classId, string status, string search)
        {
            var vm = new ClassTeacherAssignmentViewModel();

            // CLASSES
            vm.Classes = await _context.Classes
                    .AsNoTracking()
                    .Select(x => new ClassModel
                    {
                        ClassId = x.ClassId,
                        ClassName = x.ClassName
                    })
                    .ToListAsync();

            // BASE QUERY
            var query =
                _context.ClassSections
                    .AsNoTracking()
                    .Include(x => x.Class)
                    .Include(x => x.Section)
                    .Include(x => x.ClassTeacher)
                    .AsQueryable();

            // FILTERS
            if (classId.HasValue)
            {
                query = query.Where(x =>
                    x.ClassId == classId);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "assigned")
                {
                    query = query.Where(x => x.ClassTeacherId != null);
                }

                if (status == "unassigned")
                {
                    query = query.Where(x => x.ClassTeacherId == null);
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Class.ClassName.Contains(search) ||
                    x.Section.SectionName.Contains(search) ||
                    (x.ClassTeacher != null &&
                     x.ClassTeacher.FullName.Contains(search)));
            }

            // STUDENT COUNTS
            var studentCounts =
                await _context.Students
                    .GroupBy(x => x.ClassSectionId)
                    .Select(x => new
                    {
                        ClassSectionId = x.Key,
                        Count = x.Count()
                    })
                    .ToDictionaryAsync(
                        x => x.ClassSectionId,
                        x => x.Count);

            // TEACHER SUBJECTS
            var teacherSubjects =
                await _context.ClassSectionSubject
                    .Include(x => x.Subject)
                    .GroupBy(x => x.TeacherId)
                    .Select(x => new
                    {
                        TeacherId = x.Key,
                        Subject = x.Select(s => s.Subject.SubjectName).FirstOrDefault()
                    })
                    .ToDictionaryAsync(x => x.TeacherId, x => x.Subject);

            // TEACHER ASSIGNED SECTION COUNTS
            var teacherSectionCounts =
                await _context.ClassSections
                    .Where(x => x.ClassTeacherId != null)
                    .GroupBy(x => x.ClassTeacherId)
                    .Select(x => new
                    {
                        TeacherId = x.Key.Value,
                        Count = x.Count()
                    })
                    .ToDictionaryAsync(
                        x => x.TeacherId,
                        x => x.Count);

            // SECTIONS
            var sections = await query
                    .OrderBy(x => x.Class.ClassName)
                    .ThenBy(x => x.Section.SectionName)
                    .ToListAsync();

            vm.Sections =
                sections.Select(x => new SectionCardVm
                {
                    ClassSectionId = x.Id,
                    ClassName = x.Class.ClassName,
                    SectionName = x.Section.SectionName,
                    StudentCount =
                        studentCounts.ContainsKey(x.Id)
                            ? studentCounts[x.Id]
                            : 0,
                    ClassTeacher =
                        x.ClassTeacher == null
                        ? null
                        : new TeacherVm
                        {
                            TeacherId = x.ClassTeacher.TeacherID,
                            Name = x.ClassTeacher.FullName,
                            Initials = GetInitials(x.ClassTeacher.FullName),
                            SubjectName = teacherSubjects.ContainsKey(
                                    x.ClassTeacher.TeacherID)
                                ? teacherSubjects[x.ClassTeacher.TeacherID]
                                : "-",

                            TotalAssignedSections =
                                teacherSectionCounts.ContainsKey(
                                    x.ClassTeacher.TeacherID)
                                ? teacherSectionCounts[x.ClassTeacher.TeacherID]
                                : 0
                        }

                }).ToList();

            return View(vm);
        }

        public async Task<IActionResult> AssignTeacherToClass()
        {
            var vm = new ClassTeacherAssignmentViewModel();

            // LOAD CLASSES
            vm.Classes = await _context.Classes
                .Select(x => new ClassModel
                {
                    ClassId = x.ClassId,
                    ClassName = x.ClassName
                })
                .ToListAsync();

            try
            {
                var sections = await _context.ClassSections
                    .Include(x => x.Class)
                    .Include(x => x.Section)
                    .Include(x => x.ClassTeacher)
                    .Select(x => new SectionCardVm
                        {
                            ClassSectionId = x.Id,
                            ClassName = x.Class.ClassName,
                            SectionName = x.Section.SectionName,
                            StudentCount = _context.Students
                            .Count(s => s.ClassSectionId == x.Id),
                            ClassTeacher = x.ClassTeacher == null
                            ? null
                            :new TeacherVm
                            {
                                TeacherId = x.ClassTeacher.TeacherID,
                                Name = x.ClassTeacher.FullName,
                                Initials = GetInitials(x.ClassTeacher.FullName),
                                SubjectName = _context.ClassSectionSubject
                                    .Where(css => css.TeacherId == x.ClassTeacher.TeacherID)
                                    .Select(css => css.Subject.SubjectName)
                                    .FirstOrDefault(),
                                TotalAssignedSections = _context.ClassSections
                                    .Count(cs => cs.ClassTeacherId == x.ClassTeacher.TeacherID)
                            }
                        })
                        .OrderBy(x => x.ClassName)
                        .ThenBy(x => x.SectionName)
                        .ToListAsync();

                var teachers = await _context.Teachers
                    .Select(x => new TeacherVm
                    {
                        TeacherId = x.TeacherID,

                        Name = x.FullName,

                        Initials = GetInitials(x.FullName),

                        SubjectName = _context.ClassSectionSubject
                            .Where(css => css.TeacherId == x.TeacherID)
                            .Select(css => css.Subject.SubjectName)
                            .FirstOrDefault(),

                        TotalAssignedSections = _context.ClassSections
                            .Count(cs => cs.ClassTeacherId == x.TeacherID)
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync();

                vm.Sections = sections;
                vm.Teachers = teachers;

                return View(vm);
            }
            catch (Exception)
            {

            }
            return View();
        }


        // GET CLASSES
        [HttpGet]
        public async Task<IActionResult> GetClasses()
        {
            var data = await _context.Classes
                    .AsNoTracking()
                    .OrderBy(x => x.ClassName)
                    .Select(x => new
                    {
                        x.ClassId,
                        x.ClassName
                    })
                    .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSections()
        {
            var sections = await _context.Sections
                .Select(s => new
                {
                    sectionId = s.SectionId,
                    sectionName = s.SectionName
                })
                .ToListAsync();

            return Json(sections);
        }


        //[HttpGet]
        //public async Task<IActionResult> GetSectionsByClass(int classId)
        //{
        //    var sections = await _context.ClassSections
        //        .Where(x => x.ClassId == classId)
        //        .Include(x => x.Section)
        //        .Include(x => x.ClassTeacher)
        //        .ToListAsync();

        //    var data = sections.Select(x => new
        //    {
        //        classSectionId = x.Id,
        //        sectionName = x.Section.SectionName,
        //        teacherName = x.ClassTeacher != null
        //            ? x.ClassTeacher.FullName
        //            : "Unassigned"
        //    });

        //    return Json(data);
        //}


        [HttpGet]
        public async Task<IActionResult> GetSectionsByClass(int classId)
        {
            var sections = await _context.ClassSections
                .Where(x => x.ClassId == classId)
                .Include(x => x.Section)
                .Include(x => x.ClassTeacher)
                .ToListAsync();

            // IF MAPPED SECTIONS EXIST
            if (sections.Any())
            {
                var mappedData = sections.Select(x => new
                {
                    classSectionId = x.Id,
                    sectionId = x.SectionId,
                    sectionName = x.Section.SectionName,
                    teacherName = x.ClassTeacher != null
                        ? x.ClassTeacher.FullName
                        : "Unassigned",
                    isMapped = true
                });

                return Json(mappedData);
            }

            // FALLBACK → ALL SECTIONS
            var allSections = await _context.Sections
                .Select(s => new
                {
                    classSectionId = 0,
                    sectionId = s.SectionId,
                    sectionName = s.SectionName,
                    teacherName = "",
                    isMapped = false
                })
                .ToListAsync();

            return Json(allSections);
        }

        [HttpGet]
        public async Task<IActionResult> GetSectionDetails(int ClassId, int SectionId)
        {
            var students =
                await _context.StudentEnrollments
                    .CountAsync(x =>
                        x.ClassId == ClassId && x.SectionId == SectionId);

            return Json(new
            {
                students
            });
        }

        // GET TEACHERS
        [HttpGet]
        public async Task<IActionResult> GetTeachers()
        {
            var teacherCounts =
                await _context.ClassSections
                    .Where(x => x.ClassTeacherId != null)
                    .GroupBy(x => x.ClassTeacherId)
                    .Select(x => new
                    {
                        TeacherId = x.Key.Value,
                        Count = x.Count()
                    })
                    .ToDictionaryAsync(
                        x => x.TeacherId,
                        x => x.Count);

            var data =
                await _context.Teachers
                    .AsNoTracking()
                    .Select(x => new
                    {
                        TeacherId = x.TeacherID,
                        Name = x.FullName,
                        SubjectName = "-",
                        TotalAssignedSections =
                            teacherCounts.ContainsKey(x.TeacherID)
                            ? teacherCounts[x.TeacherID]
                            : 0,
                        TotalStudents = 0
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync();

            return Json(data);
        }

        // GET ASSIGNMENT
        [HttpGet]
        public async Task<IActionResult> GetAssignment(int classSectionId)
        {
            var data =
                await _context.ClassSections
                    .Where(x => x.Id == classSectionId)
                    .Select(x => new
                    {
                        ClassId = x.ClassId,
                        SectionId = x.SectionId,
                        TeacherId = x.ClassTeacherId
                    })
                    .FirstOrDefaultAsync();

            return Json(data);
        }


        // =========================================================
        // ASSIGN / CHANGE CLASS TEACHER
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAssignment(int ClassId, int SectionId, int TeacherId)
        {
            // CHECK TEACHER
            var teacherExists = await _context.Teachers
                .AnyAsync(x => x.TeacherID == TeacherId);

            if (!teacherExists)
            {
                //return BadRequest("Teacher not found.");
                TempData["Error"] = "Teacher not found.";
                return RedirectToAction("Index");
            }

            // FIND EXISTING CLASS-SECTION MAPPING
            var classSection = await _context.ClassSections
                .FirstOrDefaultAsync(x =>
                    x.ClassId == ClassId &&
                    x.SectionId == SectionId);

            if (classSection != null)
            {
                bool alreadyAssigned = await _context.ClassSections
                    .AnyAsync(x =>
                        x.ClassId == ClassId &&
                        x.SectionId == SectionId &&
                        x.Id != classSection.Id);

                if (alreadyAssigned)
                {
                    TempData["Error"] = "Section already assigned.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // IF NOT EXISTS → CREATE NEW MAPPING
            if (classSection == null)
            {
                classSection = new ClassSection
                {
                    ClassId = ClassId,
                    SectionId = SectionId,
                    ClassTeacherId = TeacherId
                };

                _context.ClassSections.Add(classSection);
            }
            else
            {
                // UPDATE EXISTING TEACHER
                classSection.ClassTeacherId = TeacherId;
            }
            
            await _context.SaveChangesAsync();
            TempData["Success"] = "Class teacher assigned successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // REMOVE CLASS TEACHER
        // =========================================================
        [HttpPost]
        public async Task<IActionResult> RemoveTeacher(int classSectionId)
        {
            var classSection = await _context.ClassSections
                .FirstOrDefaultAsync(x => x.Id == classSectionId);

            if (classSection == null)
            {
                TempData["Error"] = "Section not found.";
                //return NotFound();
                return RedirectToAction("Index");
            }

            classSection.ClassTeacherId = null;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Class teacher removed successfully.";

            return RedirectToAction(nameof(Index));
        }


    }
}
