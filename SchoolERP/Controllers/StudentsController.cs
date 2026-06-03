using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Implementations;
using SchoolERP.Services.Interfaces;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SchoolERP.Controllers
{
    [Authorize]
    public class StudentsController : Controller
    {
        private readonly IStudentProfileService _studentService;
        private readonly AppDbContext _context;
        private readonly StudentPromotionService _service;
        private readonly IPromotionService _promotionService;

        public StudentsController(
            IStudentProfileService service,
            AppDbContext context, IPromotionService promotionService,
            StudentPromotionService promoservice)
        {
            _studentService = service;
            _context = context;
            _service = promoservice;
            _promotionService = promotionService;
        }

        public async Task<IActionResult> RecentlyJoined()
        {
            DateTime startDate = new DateTime(DateTime.Now.Year, 3, 1);
            DateTime endDate = startDate.AddYears(1);

            var students = await (
                from s in _context.Students
                    // Enrollment mapping
                join se in _context.StudentEnrollments
                     on s.StudentID equals se.StudentId
                join c in _context.Classes
                        on se.ClassId equals c.ClassId
                join g in _context.Genders
                        on s.GenderID equals g.GenderID
                where s.IsActive && s.CreatedAt >= startDate &&  s.CreatedAt < endDate 
                    select new NewStudentDto
                    {
                        StudentName = s.StudentName,
                        ClassName = c.ClassName,
                        Gender = g.GenderName,
                        AdmissionNumber = s.AdmissionNumber,
                        JoinDate = s.CreatedAt,
                    }
                ) 
                .ToListAsync();

            return View(students);
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
                var userProfile = await _studentService.GetStudentProfileAsync(userId);

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
            var userProfile = await _studentService.GetStudentProfileData(userId);

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

        [HttpGet]
        public IActionResult GetAddress(int studentId)
        {
            var student = _context.Students.Find(studentId);

            if (student == null)
                return NotFound();

            var parentLink = _context.ParentStudent
                .FirstOrDefault(x => x.StudentId == student.StudentID);

            if (parentLink != null)
            {
                var parent = _context.Parents
                    .FirstOrDefault(x => x.ParentId == parentLink.ParentId);

                if (parent != null)
                {
                    return Json(new
                    {
                        address = parent.Address,
                        city = parent.City,
                        state = parent.State,
                        pincode = parent.Pincode
                    });
                }
            }

            return Json(new
            {
                address = "",
                city = "",
                state = "",
                pincode = ""
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAddress(StudentAddressDTO model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false });

            var student = _context.Students.Find(model.StudentId);

            if (student == null)
                return Json(new { success = false });

            var parentLink = _context.ParentStudent
                .FirstOrDefault(x => x.StudentId == student.StudentID);

            if (parentLink != null)
            {
                var parent =  _context.Parents
                        .FirstOrDefault(x => x.ParentId == parentLink.ParentId);

                if(parent != null)
                {
                    parent.Address = model.Address;
                    parent.City = model.City;
                    parent.State = model.State;
                    parent.Pincode = model.Pincode;
                }
                _context.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return Json(new { success = false, message = "Select a profile photo for your profile." });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                // user is not logged in or claim missing
                return Unauthorized();
            }

            // convert to int if needed
            int userId = int.Parse(userIdClaim);
            // 1. Get existing student from DB
            var existingStudent = _context.Students.FirstOrDefault(s => s.UserID == userId);

            if (existingStudent == null)
            {
                return Json(new { success = false, message = "Student not found." });
            }

            // =========================
            // DELETE OLD IMAGE (if exists)
            // =========================
            if (!string.IsNullOrEmpty(existingStudent.ProfileImagePath))
            {
                var oldFilePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    existingStudent.ProfileImagePath.TrimStart('/')
                );

                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // =========================
            // UPLOAD NEW IMAGE
            // =========================

            // Validate file type
            if (!photo.ContentType.StartsWith("image/"))
            {
                return Json(new { success = false, message = "Only image files are allowed." });
            }
            // Validate size (2MB max)
            if (photo.Length > 2 * 1024 * 1024)
            {
                return Json(new { success = false, message = "Image size must be less than 2MB." });
            }

            
            // =========================
            // 📸 Handle Image Upload
            if (photo != null && photo.Length > 0)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                string stuId = existingStudent.StudentID.ToString();
                string cleanName = Regex.Replace(existingStudent.StudentName, @"[^a-zA-Z0-9]", "");
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string extension = Path.GetExtension(photo.FileName);
                string fileName = $"{role}_{stuId}_{cleanName}_{timestamp}{extension}";

                // 👉 Student-wise folder
                var studentFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images",
                    "Students",
                    stuId // folder per existingStudent
                );
                // Ensure directory exists
                if (!Directory.Exists(studentFolder))
                {
                    Directory.CreateDirectory(studentFolder);
                }

                // 👉 Full file path
                var filePath = Path.Combine(studentFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                // ✅ IMPORTANT: Save correct relative path (including student folder)
                existingStudent.ProfileImagePath = $"/images/Students/{stuId}/{fileName}";
                await _context.SaveChangesAsync();
            }
            

            return Json(new { success = true, message = "Profile image updated successfully." });
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
                await _studentService.UpdateStudentProfileAsync(model);
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

        //public IActionResult Index(StudentListViewModel vm)
        //{
        //    vm.Classes = _context.Classes.Select(c => new SelectListItem
        //    {
        //        Value = c.ClassId.ToString(),
        //        Text = c.ClassName
        //    }).ToList();

        //    vm.Sections = _context.Sections
        //       .Select(s => new SelectListItem
        //       {
        //           Value = s.SectionId.ToString(),
        //           Text = s.SectionName
        //       }).ToList();

        //    var query = from s in _context.Students
        //                join cs in _context.ClassSections on s.ClassSectionId equals cs.Id
        //                join c in _context.Classes on cs.ClassId equals c.ClassId
        //                join sec in _context.Sections on cs.SectionId equals sec.SectionId
        //                select new StudentDto
        //                {
        //                    PhoneNumber = s.PhoneNo,
        //                    StudentID = s.StudentID,
        //                    RollNumber = s.RollNumber,
        //                    StudentName = s.StudentName,
        //                    Gender = s.GenderID,
        //                    ClassSectionID = cs.Id,
        //                    ClassID = c.ClassId,
        //                    ClassName = c.ClassName,
        //                    SectionID = sec.SectionId,
        //                    SectionName = sec.SectionName
        //                };

        //    if (vm.ClassID.HasValue)
        //        query = query.Where(x => x.ClassID == vm.ClassID.Value);

        //    if (vm.SectionID.HasValue)
        //        query = query.Where(x => x.SectionID == vm.SectionID.Value);

        //    if (!string.IsNullOrEmpty(vm.RollNumber))
        //        query = query.Where(x => x.RollNumber.Contains(vm.RollNumber));

        //    vm.Students = query.ToList();

        //    return View(vm);
        //}


        public IActionResult Index(int? classId, int? sectionId, string? rollNumber)
        {
            StudentListViewModel vm = new()
            {
                ClassID = classId,
                SectionID = sectionId,
                RollNumber = rollNumber
            };

            vm.Classes = _context.Classes.Select(c => new SelectListItem
            {
                Value = c.ClassId.ToString(),
                Text = c.ClassName
            }).ToList();

            vm.Sections = _context.Sections.Select(s => new SelectListItem
            {
                Value = s.SectionId.ToString(),
                Text = s.SectionName
            }).ToList();

            var query = from s in _context.Students
                        join cs in _context.ClassSections on s.ClassSectionId equals cs.Id
                        join c in _context.Classes on cs.ClassId equals c.ClassId
                        join sec in _context.Sections on cs.SectionId equals sec.SectionId
                        select new StudentDto
                        {
                            IsActive = s.IsActive,
                            PhoneNumber = s.PhoneNo,
                            EmilID = s.EmailID,
                            StudentID = s.StudentID,
                            AdmissionNumber = s.AdmissionNumber,
                            RollNumber = s.RollNumber,
                            StudentName = s.StudentName,
                            Gender = s.GenderID,
                            ClassSectionID = cs.Id,
                            ClassID = c.ClassId,
                            ClassName = c.ClassName,
                            SectionID = sec.SectionId,
                            SectionName = sec.SectionName,
                            JoinDate = s.CreatedAt.ToString("dd MMM yyyy")
                        };

            if (classId.HasValue)
                query = query.Where(x => x.ClassID == classId.Value);

            if (sectionId.HasValue)
                query = query.Where(x => x.SectionID == sectionId.Value);

            if (!string.IsNullOrWhiteSpace(rollNumber))
                query = query.Where(x => x.RollNumber.Contains(rollNumber));

            vm.Students = query.ToList();

            return View(vm);
        }

        public ActionResult Details(int id)
        {
            var student = (from s in _context.Students

                           join se in _context.StudentEnrollments
                                on s.StudentID equals se.StudentId
                           join c in _context.Classes
                               on se.ClassId equals c.ClassId
                           join sec in _context.Sections
                               on se.SectionId equals sec.SectionId

                           where s.StudentID == id && se.Status == "Active"

                           select new StudentFormViewModel
                           {
                               // Student Info
                               RollNumber = s.RollNumber,
                               StudentName = s.StudentName,
                               GenderID = s.GenderID,
                               DateOfBirth = s.DateOfBirth,
                               PhoneNo = s.PhoneNo,
                               EmailID = s.EmailID,
                               BloodGroup = s.BloodGroup,
                               Aadhaar = s.Aadhaar,
                               ApaarID = s.ApaarID ?? "N/A",

                               // Parent Info
                               PermanentAddress = s.ParentStudents
                                   .Select(ps => ps.Parent.Address)
                                   .FirstOrDefault(),

                               FatherName = s.ParentStudents
                               .Select(ps => ps.Parent.FatherName)
                               .FirstOrDefault(),

                               MotherName = s.ParentStudents
                       .Select(ps => ps.Parent.MotherName)
                       .FirstOrDefault(),

                               FatherAadhaar = s.ParentStudents
                       .Select(ps => ps.Parent.FatherAadhaar)
                       .FirstOrDefault(),

                               MotherAadhaar = s.ParentStudents
                       .Select(ps => ps.Parent.MotherAadhaar)
                       .FirstOrDefault(),

                               EmergencyContact = s.ParentStudents
                       .Select(ps => ps.Parent.EmergencyContact)
                       .FirstOrDefault(),

                               // Academic Info
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


            ViewBag.ClassList = _context.Classes
            .OrderBy(c => c.ClassOrder)
            .Select(c => new
            {
                c.ClassId,
                c.ClassName,
                c.ClassOrder
            })
            .ToList();

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

            //vm.Classes = _context.Classes.
            //    OrderBy(c => c.ClassOrder)
            //    .Select(c => new SelectListItem
            //{
            //    Value = c.ClassId.ToString(),
            //    Text = c.ClassName
            //}).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _studentService.GetStudentForEditAsync(id);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var studentExists = await _context.Students
                    .AnyAsync(x => x.StudentID == id);

                var student = await _context.Students
                    .FirstOrDefaultAsync(x => x.StudentID == id);

                if (student == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Student not found."
                    });
                }

                //_context.Students.Remove(student);
                student.IsActive = false;
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Student deactivated successfully."
                });
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(new
                {
                    success = false,
                    message = "Something went wrong, try after sometime."
                });
            }
        }


        [HttpGet]
        public JsonResult GetSectionsByClass(int classId)
        {
            var sections = (from cs in _context.ClassSections
                            join s in _context.Sections
                                on cs.SectionId equals s.SectionId
                            where cs.ClassId == classId
                            select new
                            {
                                id = s.SectionId,
                                name = s.SectionName
                            }).ToList();

            return Json(sections);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentFormViewModel model)
        {
            // Skip validation for PaymentOption
            ModelState.Remove("PaymentOption");

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

            if (!ModelState.IsValid)
            {
                // reload only dropdowns
                model = await _studentService.RebuildEditModelAsync(model);
                return View(model);
            }

            var result = await _studentService.UpdateStudentAsync(model);

            if (!result)
            {
                ModelState.AddModelError("", "Failed to update existingStudent.");
                model = await _studentService.RebuildEditModelAsync(model);
                return View(model);
            }

            TempData["success"] = "Student data updated successfully.";

            //return RedirectToAction("Index");

            return RedirectToAction("Index", new
            {
                classId = (int?)null,
                sectionId = (int?)null,
                rollNumber = (string?)null
            });
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
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

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
                CreateStudentResultDto resultDto  = await _studentService.SaveStudentDetail(vm);

                //vm.RollNumber = resultDto.RollNumber;
                //TempData["RollNumber"] = resultDto.RollNumber;
                //TempData["UserName"] = resultDto.UserName;
                //TempData["Password"] = "Password@123"; // plain password (if you return it)
                //TempData["IsSuccess"] = true;


                if (resultDto != null && resultDto.Status == 1) {
                    // Create object for modal
                    var modalData = new
                    {
                        IsSuccess = true,
                        StudentId = resultDto.StudentID,
                        Message = "Student Created Successfully",
                        RollNumber = resultDto.RollNumber,
                        UserName = resultDto.UserName,
                        Password = "Password@123"
                    };

                    TempData["StudentCreated"] = JsonConvert.SerializeObject(modalData);
                    return RedirectToAction("NewStudent"); // ✅ redirect to same page
                }
                else {

                    var modalData = new
                    {
                        IsSuccess = false,
                        Message = resultDto.Message
                    };

                    TempData["StudentCreated"] = JsonConvert.SerializeObject(modalData);
                }
            }
            
            // IMPORTANT: Reload dropdowns if validation fails
            LoadDropdowns(vm); // 👈 reusable call

            return View(vm);
        }
       
        public async Task<JsonResult> OnGetPaymentDetails(int studentId)
        {
            // 1. Get latest fee (better: filter by AcademicYear if needed)
            var fee = await _context.StudentFees
                .Include(sf => sf.AcademicYear)
                .Where(stu => stu.StudentId == studentId)
                .OrderByDescending(stu => stu.FeeId)
                .FirstOrDefaultAsync();

            var dueDay = _context.FeeConfigurations.Select(x => x.DueDay).FirstOrDefault();

            return new JsonResult(new { message = "" });
        }


        public async Task<IActionResult> Promotion()
        {
            var year = await _context.AcademicYears
               .FirstOrDefaultAsync(x => x.IsActive);

            var state = await _promotionService.GetPromotionStateAsync(year.AcademicYearID);

            if (!state.CanAccess)
                return RedirectToAction("AccessDenied", "Home");

            //var currentYear = await _context.AcademicYears
            //    .Where(x => x.IsActive)
            //    .Select(x => x.AcademicYearID)
            //    .FirstOrDefaultAsync();

            var vm = new PromotionViewModel
            {
                FromAcademicYearId = year.AcademicYearID,
                ToAcademicYearId = year.AcademicYearID,

                Classes = await _context.Classes
                    .Select(c => new SelectListItem
                    {
                        Value = c.ClassId.ToString(),
                        Text = c.ClassName
                    }).ToListAsync(),

                AcademicYears = await _context.AcademicYears
                    .Select(a => new SelectListItem
                    {
                        Value = a.AcademicYearID.ToString(),
                        Text = a.YearName,
                        Selected = a.IsActive   // optional if using asp-for
                    }).ToListAsync(),

                Sections = new List<SelectListItem>() // initially empty
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentsForPromotion(int classId, int sectionId)
        {
            try
            {
                if (classId <= 0 || sectionId <= 0)
                    return BadRequest("Invalid class or section.");

                // 1. Get current academic year
                var academicYear = await _context.AcademicYears
                    .FirstOrDefaultAsync(x => x.IsActive);

                if (academicYear == null)
                    return BadRequest("No active academic year found.");

                // 2. Get students in current class
                var students = await _context.StudentEnrollments
                    .Where(e =>
                        e.ClassId == classId &&
                        e.SectionId == sectionId &&
                        e.AcademicYearId == academicYear.AcademicYearID &&
                        e.Status == "Active" &&
                        e.IsActive)
                    .Select(e => new
                    {
                        studentId = e.StudentId,
                        name = e.Student.StudentName,
                        rollNo = e.RollNumber,
                        enrollmentId = e.EnrollmentId
                    })
                    .OrderBy(x => x.rollNo)
                    .ToListAsync();

                return Json(students);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error loading students: " + ex.Message);
            }
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


        [HttpPost]
        public async Task<IActionResult> PromoteStudents([FromBody] PromotionViewModel model)
        {
            var studentid = GetLoggedInStudentId(); // get from session/login

            var result = await _service.PromoteStudentsAsync(model, studentid);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }

    }
}
