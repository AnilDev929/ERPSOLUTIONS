using ERP_SOLUTIONS.Data;
using ERP_SOLUTIONS.Models.DTOS;
using ERP_SOLUTIONS.Models.Entities;
using ERP_SOLUTIONS.Models.ViewModels;
using ERP_SOLUTIONS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ERP_SOLUTIONS.Controllers
{
    [Authorize]
    public class StudentsController : Controller
    {
        private readonly IStudentProfileService _service;
        private readonly AppDbContext _context;

        public StudentsController(IStudentProfileService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }


        // GET: /Students/MyProfile
        //[Authorize] // ensure only logged-in users can access
        public async Task<IActionResult> MyProfile()
        {
            try
            {
                // Get the current logged-in user (based on username/email)
                var userName = User.Identity.Name;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    // user is not logged in or claim missing
                    return Unauthorized();
                }
                
                // convert to int if needed
                int userId = int.Parse(userIdClaim);
                //var userProfile = await _service.GetStudentProfileAsync(userId);

                //if (userProfile == null)
                //    return View("Error"); // or NotFound page

                //return View(userProfile); // pass model to view

                return View();
            }
            catch (Exception)
            {
                // Log exception
                //_logger.LogError(ex, "Error loading profile for user {UserName}", User.Identity.Name);
                return View("Error");
            }
        }

        // GET: /Account/EditProfile
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var userName = User.Identity.Name;
            var userProfile = await _service.GetStudentProfileAsync(1);

            if (userProfile == null)
                return View("Error");

            return View(userProfile);
        }

        // POST: /Account/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EditProfile(Student model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _service.UpdateStudentProfileAsync(model);
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("MyProfile");
            }
            catch (Exception)
            {
                //_logger.LogError(ex, "Error updating profile for user {UserName}", User.Identity.Name);
                ModelState.AddModelError("", "Failed to update profile. Please try again later.");
                return View(model);
            }
        }

        public IActionResult Entry()
        {
            return View();
        }

        private void LoadDropdowns(StudentFormViewModel vm)
        {
            var activeYearId = _context.AcademicYears
            .Where(a => a.IsActive)
            .Select(a => a.AcademicYearID)
            .FirstOrDefault();

            vm.AcademicYearID = activeYearId; // 👈 pre-select

            vm.Genders = _context.Genders.Select(g => new SelectListItem
            {
                Value = g.GenderID.ToString(),
                Text = g.GenderName
            }).ToList();

            vm.AcademicYears = _context.AcademicYears.Select(a => new SelectListItem
            {
                Value = a.AcademicYearID.ToString(),
                Text = a.YearName,
                Selected = a.IsActive // auto-select active
            }).ToList();

            vm.Classes = _context.Classes.Select(c => new SelectListItem
            {
                Value = c.ClassId.ToString(),
                Text = c.ClassName
            }).ToList();
        }

        [HttpGet]
        public IActionResult NewStudent()
        {
            var vm = new StudentFormViewModel();
            LoadDropdowns(vm); // 👈 reusable call
            return View(vm);
        }

        //public IActionResult GetClassDetails(int classId, int academicYearId)
        //{
        //    var sections = _context.ClassSections
        //        .Where(cs => cs.ClassId == classId)
        //        .Select(cs => new { cs.SectionId, cs.Section.SectionName })
        //        .ToList();

        //    var fees = _context.CourseFees
        //        .Where(cf => cf.ClassId == classId && cf.AcademicYearId == academicYearId)
        //        .Select(cf => new { cf.TuitionFee, cf.LabFee, cf.LibraryFee, cf.OtherFee, cf.TotalFee })
        //        .FirstOrDefault();

        //    return Json(new { sections, fees });
        //}
        [HttpGet]
        public IActionResult GetClassDetails(int classId, int academicYearId)
        {
            // Get all sections for this class
            var sections = _context.ClassSections
                .Where(cs => cs.ClassId == classId)
                .Select(cs => new
                {
                    cs.Id,             // ClassSectionId
                    SectionId = cs.SectionId,
                    SectionName = cs.Section.SectionName
                }).ToList();

            // Get course fees for this class + academic year
            var fees = _context.CourseFees
                .Where(cf => cf.ClassId == classId && cf.AcademicYearId == academicYearId)
                .Select(cf => new
                {
                    cf.TuitionFee,
                    cf.LabFee,
                    cf.LibraryFee,
                    cf.OtherFee,
                    cf.TotalFee
                }).FirstOrDefault();

            return Json(new { sections, fees });
        }

        [HttpPost]
        public async Task<IActionResult> NewStudent(StudentFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Loop through all ModelState entries
                foreach (var entry in ModelState)
                {
                    var key = entry.Key; // The field name
                    foreach (var error in entry.Value.Errors)
                    {
                        // This prints to console (for debugging)
                        Console.WriteLine($"Field: {key}, Error: {error.ErrorMessage}");
                    }
                }
            }

            vm.Genders = _context.Genders.Select(g => new SelectListItem
            {
                Value = g.GenderID.ToString(),
                Text = g.GenderName
            }).ToList();

            vm.AcademicYears = _context.AcademicYears.Select(a => new SelectListItem
            {
                Value = a.AcademicYearID.ToString(),
                Text = a.YearName,
                Selected = a.IsActive // auto-select active
            }).ToList();

            vm.Classes = _context.Classes.Select(c => new SelectListItem
            {
                Value = c.ClassId.ToString(),
                Text = c.ClassName
            }).ToList();


            if (ModelState.IsValid)
            {
                CreateStudentResultDto resultDto  = await _service.SaveStudentDetail(vm);

                vm.RollNumber = resultDto.RollNumber;


                return RedirectToAction("Index");
            }

            // IMPORTANT: Reload dropdowns if validation fails
            LoadDropdowns(vm); // 👈 reusable call

            return View(vm);
        }

    }
}
