using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;

namespace SchoolERP.Controllers
{
    [Authorize(Roles = "Admin")] // 🔒 IMPORTANT
    public class MappingController : Controller
    {
        private readonly AppDbContext _context;

        public MappingController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Dropdowns
        public IActionResult GetDropdowns()
        {
            var data = new
            {
                classes = _context.Classes.Select(x => new { id = x.ClassId, name = x.ClassName }).ToList(),
                sections = _context.Sections.Select(x => new { id = x.SectionId, name = x.SectionName }).ToList(),
                subjects = _context.Subjects.Select(x => new { id = x.SubjectId, name = x.SubjectName + " (" + x.SubjectCode + ")" }).ToList(),
                teachers = _context.Teachers.Select(x => new { id = x.TeacherID, name = x.FullName }).ToList()
            };

            return Json(data);
        }

        // Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] MappingRequest request)
        {
            if (request.ClassId == 0 || request.SectionId == 0)
            {
                return Json(new { success = false, message = "Please select class and section." });
            }

            try {

                //“check exists → if not exists → insert”.
                var classSection = await _context.ClassSections
                    .FirstOrDefaultAsync(x =>
                    x.ClassId == request.ClassId &&
                    x.SectionId == request.SectionId);

                //if (classSection == null) return BadRequest();
                if (classSection == null)
                {
                    classSection = new ClassSection
                    {
                        ClassId = request.ClassId,
                        SectionId = request.SectionId,
                        ClassTeacherId = 0
                    };

                    _context.ClassSections.Add(classSection);
                    await _context.SaveChangesAsync();
                }

                var existing = await _context.ClassSectionSubject
                    .Where(x => x.ClassSectionId == classSection.Id)
                    .ToListAsync();

                var incoming = request.Mappings
               .Select(x => new { x.SubjectId, x.TeacherId })
               .ToList();

                // ---------------------------
                // 1. DELETE missing items
                // ---------------------------
                var toDelete = existing
                    .Where(e => !incoming.Any(i =>
                        i.SubjectId == e.SubjectId &&
                        i.TeacherId == e.TeacherId))
                    .ToList();

                _context.ClassSectionSubject.RemoveRange(toDelete);

                // ---------------------------
                // 2. ADD new items
                // ---------------------------
                var toAdd = incoming
                    .Where(i => !existing.Any(e =>
                        e.SubjectId == i.SubjectId &&
                        e.TeacherId == i.TeacherId))
                    .Select(i => new ClassSectionSubject
                    {
                        ClassSectionId = classSection.Id,
                        SubjectId = i.SubjectId,
                        TeacherId = i.TeacherId
                    })
                    .ToList();

                await _context.ClassSectionSubject.AddRangeAsync(toAdd);

                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = "Subject mapping saved successfully."
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "Error while saving mapping. Please try again."
                });
            }
        }

        // Get existing
        public IActionResult GetMappings(int classId, int sectionId)
        {
            var classSection = _context.ClassSections
                .FirstOrDefault(x => x.ClassId == classId && x.SectionId == sectionId);

            if (classSection == null) return Json(new List<object>());

            var data = _context.ClassSectionSubject
                .Where(x => x.ClassSectionId == classSection.Id)
                .Select(x => new
                {
                    subjectId = x.SubjectId,
                    subjectName = x.Subject.SubjectName,
                    teacherId = x.TeacherId,
                    teacherName = x.Teacher.FullName
                }).ToList();

            return Json(data);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClass(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "Class name is required" });

            name = name.Trim();

            var exists = await _context.Classes
                .AnyAsync(x => x.ClassName == name);

            if (exists)
                return Json(new { success = false, message = "Class already exists" });

            var model = new ClassModel
            {
                ClassName = name
            };

            _context.Classes.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Class added successfully" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSection(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "Section name is required" });

            name = name.Trim();

            var exists = await _context.Sections
                .AnyAsync(x => x.SectionName.ToUpper() == name.ToUpper());

            if (exists)
                return Json(new { success = false, message = "Section name already exists." });

            var model = new Section
            {
                SectionName = name
            };

            _context.Sections.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "New subject has been created successfully." });
        }

        [HttpGet]
        public IActionResult GetClasses()
        {
            var classes = _context.Classes
                .Select(c => new
                {
                    id = c.ClassId,
                    name = c.ClassName
                })
                .ToList();

            return Json(classes);
        }

        [HttpGet]
        public IActionResult GetSections(int classId)
        {
            var sections = _context.Sections
                .Select(s => new
                {
                    id = s.SectionId,
                    name = s.SectionName
                })
                .ToList();

            return Json(sections);
        }















    }
}
