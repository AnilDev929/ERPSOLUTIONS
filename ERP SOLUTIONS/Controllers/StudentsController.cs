using ERP_SOLUTIONS.Data;
using ERP_SOLUTIONS.Models.DTOS;
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
                var userProfile = await _service.GetStudentProfileAsync(userId);

                //if (userProfile == null)
                //    return View("Error"); // or NotFound page

                //return View(userProfile); // pass model to view

                return View(userProfile);
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = int.Parse(userIdClaim);
            var userProfile = await _service.GetStudentProfileData(userId);

            // Bind Gender List
            ViewBag.Genders = _context.Genders
                .Select(g => new SelectListItem
                {
                    Value = g.GenderID.ToString(),
                    Text = g.GenderName
                }).ToList();

            if (userProfile == null)
                return View("Error");

            return View(userProfile);
        }

        // POST: /Account/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EditProfile(EditStudentProfileDto model)
        {
            if (!ModelState.IsValid)
            {
                //// Loop through all ModelState entries
                //foreach (var entry in ModelState)
                //{
                //    var key = entry.Key; // The field name
                //    foreach (var error in entry.Value.Errors)
                //    {
                //        // This prints to console (for debugging)
                //        Console.WriteLine($"Field: {key}, Error: {error.ErrorMessage}");
                //    }
                //}

                ViewBag.Genders = _context.Genders
                    .Select(g => new SelectListItem
                    {
                        Value = g.GenderID.ToString(),
                        Text = g.GenderName
                    }).ToList();

                return View(model);
            }

            try
            {
                await _service.UpdateStudentProfileAsync(model);
                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction("MyProfile");
            }
            catch (Exception)
            {
                //_logger.LogError(ex, "Error updating profile for user {UserName}", User.Identity.Name);
                TempData["Success"] = "Failed to update profile. Please try again later.!";
                return View(model);
            }
        }

        public IActionResult Entry(StudentListViewModel vm)
        {
            vm.Classes = _context.Classes.Select(c => new SelectListItem
            {
                Value = c.ClassId.ToString(),
                Text = c.ClassName
            }).ToList();

            vm.Sections = _context.Sections
               .Select(s => new SelectListItem
               {
                   Value = s.SectionId.ToString(),
                   Text = s.SectionName
               }).ToList();

            //var query = _context.Students.AsQueryable();

            var query = from s in _context.Students
                        join cs in _context.ClassSections on s.ClassSectionId equals cs.Id
                        join c in _context.Classes on cs.ClassId equals c.ClassId
                        join sec in _context.Sections on cs.SectionId equals sec.SectionId
                        select new StudentDto
                        {
                            PhoneNumber = s.PhoneNo,
                            StudentID = s.StudentID,
                            RollNumber = s.RollNumber,
                            StudentName = s.StudentName,
                            Gender = s.GenderID,
                            ClassSectionID = cs.Id,
                            ClassID = c.ClassId,
                            ClassName = c.ClassName,
                            SectionID = sec.SectionId,
                            SectionName = sec.SectionName
                        };

            if (vm.ClassID.HasValue)
                query = query.Where(x => x.ClassID == vm.ClassID.Value);

            if (vm.SectionID.HasValue)
                query = query.Where(x => x.SectionID == vm.SectionID.Value);

            if (!string.IsNullOrEmpty(vm.RollNumber))
                query = query.Where(x => x.RollNumber.Contains(vm.RollNumber));

            vm.Students = query.ToList();

            return View(vm);
        }

        public ActionResult Details(int id)
        {
            // Get student + related Class & Section names
            var student = (from s in _context.Students
                join cs in _context.ClassSections
                    on s.ClassSectionId equals cs.Id
                join c in _context.Classes
                    on cs.ClassId equals c.ClassId
                join sec in _context.Sections
                    on cs.SectionId equals sec.SectionId
                where s.StudentID == id
                select new StudentFormViewModel
                {
                    RollNumber = s.RollNumber,
                    StudentName = s.StudentName,
                    GenderID = s.GenderID,
                    DateOfBirth = s.DateOfBirth,
                    PermanentAddress = s.PermanentAddress,
                    PhoneNo = s.PhoneNo,
                    EmailID = s.EmailID,
                    BloodGroup = s.BloodGroup,
                    Aadhaar = s.Aadhaar,
                    ApaarID = s.ApaarID ?? "N/A",
                    FatherName = s.FatherName,
                    MotherName = s.MotherName,
                    FatherAadhaar = s.FatherAadhaar,
                    MotherAadhaar = s.MotherAadhaar,
                    EmergencyContact = s.EmergencyContact,
                    ClassName = c.ClassName,
                    SectionName = sec.SectionName
                }).FirstOrDefault();

            if (student == null)
                return Content("<div class='text-danger'>Student not found</div>");
            

            return PartialView("_StudentDetails", student);
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
                    cf.CourseFeeId,
                    cf.TuitionFee,
                    cf.AdmissionFee,
                    cf.TransportFee,
                    cf.LibraryFee,
                    cf.OtherFee,
                    cf.TotalFee
                }).FirstOrDefault();

            return Json(new { sections, fees });
        }

        [HttpPost]
        public async Task<IActionResult> NewStudent(StudentFormViewModel vm)
        {
            //if (!ModelState.IsValid)
            //{
            //    // Loop through all ModelState entries
            //    foreach (var entry in ModelState)
            //    {
            //        var key = entry.Key; // The field name
            //        foreach (var error in entry.Value.Errors)
            //        {
            //            // This prints to console (for debugging)
            //            Console.WriteLine($"Field: {key}, Error: {error.ErrorMessage}");
            //        }
            //    }
            //}


            if (ModelState.IsValid)
            {
                CreateStudentResultDto resultDto  = await _service.SaveStudentDetail(vm);

                vm.RollNumber = resultDto.RollNumber;
                TempData["Success"] = $"Student created successfully! Roll No: {resultDto.RollNumber}";

                TempData["RollNumber"] = resultDto.RollNumber;
                TempData["UserName"] = resultDto.UserName;
                TempData["Password"] = "Password@123"; // plain password (if you return it)
                TempData["IsSuccess"] = true;

                return RedirectToAction("NewStudent"); // ✅ redirect to same page
            }

            // ❗ Add this
            TempData["IsSuccess"] = false;
            TempData["Error"] = "Something went wrong. Please check inputs.";
            // IMPORTANT: Reload dropdowns if validation fails
            LoadDropdowns(vm); // 👈 reusable call

            return View(vm);
        }

    }
}
