using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Enum;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;
using System.Drawing;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace SchoolERP.Controllers
{
    [Authorize]
    public class TeacherController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly AppDbContext _context;
        private readonly ILogger<TeacherController> _logger;
        private readonly IDataProtector _protector;

        public TeacherController(ITeacherService teacherService, AppDbContext context, 
            ILogger<TeacherController> logger, IDataProtectionProvider provider)
        {
            _teacherService = teacherService;
            _context = context;
            _logger = logger;
            _protector = provider.CreateProtector("TeacherIdProtector");
        }

        [Authorize(Roles = "Teacher")] // 🔒 IMPORTANT
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            TeacherProfileDTO teacher = await _teacherService.GetProfileByUserIdAsync(Convert.ToInt32(userId));
            return View(teacher);
        }

        public async Task<IActionResult> UpdateProfile(int id)
        {
            //var teachers = await _teacherService.GetAllAsync();
            //return View(teachers);
            var teacherProfile = await _teacherService.GetProfileAsync(id);
            if (teacherProfile == null) return NotFound();

            return View(teacherProfile.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(TeacherProfileUpdateDTO model, IFormFile profileImageInput)
        {
            try
            {
                // 🔎 Model validation
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please correct the highlighted errors.";
                    return View(model);
                }

                // 🔐 Get logged-in user
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var teacher = await _context.Teachers.FindAsync(model.Id);
                string oldImagePath = teacher.ProfileImageUrl;

                // =========================
                // CASE 2: If IMAGE CHANGED
                // =========================
                // 📸 Handle Image Upload
                if (profileImageInput != null && profileImageInput.Length > 0)
                {
                    // Validate file type
                    if (!profileImageInput.ContentType.StartsWith("image/"))
                    {
                        ModelState.AddModelError("ProfileImageUrl", "Only image files are allowed.");
                        return View(model);
                    }

                    // Validate size (2MB max)
                    if (profileImageInput.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ProfileImageUrl", "Image size must be less than 2MB.");
                        return View(model);
                    }

                    // Generate unique file name
                    var role = User.FindFirst(ClaimTypes.Role)?.Value;
                    string empId = model.Id.ToString();
                    string cleanName = Regex.Replace(model.FullName, @"[^a-zA-Z0-9]", "");
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string extension = Path.GetExtension(model.ProfileImageInput.FileName);

                    string fileName = $"{role}_{empId}_{cleanName}_{timestamp}{extension}";

                    //var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImageInput.FileName);

                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");

                    // Ensure directory exists
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImageInput.CopyToAsync(stream);
                    }
                                        
                    // Save relative path to DB
                    model.ProfileImageUrl = "/images/profiles/" + fileName;
                }
                // =========================
                // CASE 1: IMAGE NOT CHANGED
                // =========================
                else
                {
                    // keep existing image
                    teacher.ProfileImageUrl = oldImagePath;
                }

                // 📦 Call service
                var result = await _teacherService.UpdateProfileAsync(model, Convert.ToInt32(userId));

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                // =========================
                // 🗑 DELETE OLD IMAGE ONLY IF NEW UPLOADED
                // =========================
                if (model.ProfileImageInput != null &&
                    !string.IsNullOrEmpty(oldImagePath) &&
                    oldImagePath != "/images/default-user.png")
                {
                    string fullOldPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        oldImagePath.TrimStart('/'));

                    if (System.IO.File.Exists(fullOldPath))
                    {
                        System.IO.File.Delete(fullOldPath);
                    }
                }

                TempData["Success"] = result.Message;
                //return RedirectToAction("UpdateProfile");
                return RedirectToAction("UpdateProfile", new { id = model.Id });
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong while updating profile.";
                return View(model);
            }
        }

        [Authorize(Roles = "Admin, Teacher")] // 🔒 IMPORTANT
        public async  Task<IActionResult> Index(string search, int page = 1, int pageSize = 10)
        {
            // Start with all teachers
            var teachers = await _teacherService.GetAllAsync();
            //var teachers = _context.Teachers.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                teachers = teachers.Where(t =>
                    t.TeacherName.Contains(search) ||
                    t.Phone.Contains(search) ||
                    t.EmailID.Contains(search) ||
                    t.Qualification.Contains(search));
            }

            // Count total records after filter
            int totalRecords = teachers.Count();

            // Apply paging
            var pagedTeachers = teachers
                .OrderBy(t => t.TeacherName)           // Sort by name
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pass data + pagination info to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.Search = search;

            return View(pagedTeachers);
        }

        [Authorize(Roles = "Admin")] // 🔒 IMPORTANT
        public IActionResult Create()
        {
            return View(new Teacher());
        }

        [Authorize(Roles = "Admin")] // 🔒 IMPORTANT
        [HttpPost]
        public async Task<IActionResult> Create(Teacher model)
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

            if (ModelState.IsValid)
            {
                try
                {
                    var data = await _teacherService.CreateTeacherWithUserAsync(model);

                    if(data.username == "")
                    {
                        TempData["Error"] = "Teacher already exists.";
                        return View(model);
                    }
                    TempData["Success"] = $"Teacher {model.FullName} added successfully! Username: {data.username}, and Password: {data.password}";
                    return RedirectToAction("Index", new { search = "", page = 1, pageSize = 10 });
                }
                catch ( Exception ex)
                {
                    TempData["Error"] = ex.Message;
                    return RedirectToAction("Index", new { search = "", page = 1, pageSize = 10 });
                }
            }
            return View(model);
        }

        [Authorize(Roles = "Admin, Teacher")] // 🔒 IMPORTANT
        public async Task<IActionResult> Edit(int teacherID)
        {
            //var decrypted = _protector.Unprotect(teacherID);
            //int teacherId = Convert.ToInt32(decrypted);

            var teacher = await _teacherService.GetByIdAsync(Convert.ToInt32(teacherID));
            return View("Create", teacher);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Teacher model)
        {
            var isTeacher = User.IsInRole("Teacher");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 🔐 Authorization check (early exit)
            if (isTeacher && model.UserID.ToString() != userId)
                return Unauthorized();

            // 🎯 Call service
            var result = await _teacherService.UpdateAsync(model, isTeacher ? "Teacher" : "Admin");

            // ❌ Failure flow
            if (!result.Status)
            {
                return isTeacher
                    ? RedirectToAction("Create", model)
                    : View("Create", model);
            }

            // ✅ Success flow
            return isTeacher
                ? RedirectToAction("Profile")
                : RedirectToAction("Index", new { search = "", page = 1, pageSize = 10 });
        }

        public async Task<IActionResult> Deactivate(int id)
        {
            await _teacherService.DeactivateAsync(id);
            return RedirectToAction("Index", new { search = "", page = 1, pageSize = 10 });
        }

        [HttpPost]
        //public async Task<IActionResult> UpdateStatus([FromBody] UpdateTeacherStatusDto model)
        public async Task<IActionResult> UpdateStatus([FromBody] JsonElement data)        
        {
            var teacherID = data.GetProperty("TeacherID").GetInt32();
            var status = data.GetProperty("Status").GetString();
            var remark = data.GetProperty("StatusRemark").GetString();

            System.Enum.TryParse(status, true, out TeacherStatus c);

            try
            {
                var teacher = await _context.Teachers
                .FindAsync(teacherID);

                if (teacher == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Teacher not found"
                    });
                }

                //Update teacher active status
                teacher.Status = c;
                teacher.StatusRemark = remark;
                teacher.StatusUpdatedOn = DateTime.Now;
                if(c == TeacherStatus.Active)
                    teacher.IsActive = true;
                else
                    teacher.IsActive = false;

                //Update user active status
                var user = await _context.Users.FindAsync(teacher.UserID);
                if (c == TeacherStatus.Active)
                    user.IsActive = true;
                else
                    user.IsActive = false;

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Status updated successfully"
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "Something went wrong."
                });
            }
            
        }





    }
}
