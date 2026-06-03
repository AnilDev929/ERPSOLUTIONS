using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Enum;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.ViewModels;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SchoolERP.Controllers
{
    public class ParentController : Controller
    {
        private readonly AppDbContext _context;

        public ParentController(AppDbContext context) 
        { 
            _context = context; 
        }

        public async Task<IActionResult> Profile()
        {
            // Logged-in parent id
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                // user is not logged in or claim missing
                return Unauthorized();
            }
            // convert to int
            int parentId = int.Parse(userIdClaim);

            // -----------------------------------
            // STEP 1: LOAD PARENT DETAILS
            // -----------------------------------
            var parent = await _context.Parents
                .AsNoTracking()
                .Where(x => x.ParentId == parentId)
                .Select(x => new
                {
                    x.FatherName,
                    x.ParentEmail,
                    x.Phone,
                    x.Address
                })
                .FirstOrDefaultAsync();

            if (parent == null)
            {
                return NotFound();
            }

            // -----------------------------------
            // STEP 2: LOAD CHILDREN DASHBOARD DATA
            // -----------------------------------
            var students = await GetParentDashboardAsync(parentId);

            // -----------------------------------
            // STEP 3: POPULATE VIEWMODEL
            // -----------------------------------
            var model = new ParentProfileViewModel
            {
                ParentId= parentId,
                ParentName = parent.FatherName,
                Email = parent.ParentEmail,
                MobileNumber = parent.Phone,
                Address = parent.Address,

                Students = students
            };

            // -----------------------------------
            // STEP 4: RETURN VIEW
            // -----------------------------------
            return View(model);
        }

        private async Task<List<StudentDto>> GetParentDashboardAsync(int parentId)
        {
            try
            {
                // -------------------------------
                // STEP 1: GET STUDENTS OF PARENT
                // -------------------------------
                var students = await (
                    from ps in _context.ParentStudent.AsNoTracking()
                    join s in _context.Students.AsNoTracking()
                        on ps.StudentId equals s.StudentID
                    join se in _context.StudentEnrollments.AsNoTracking()
                        on s.StudentID equals se.StudentId
                    join c in _context.Classes.AsNoTracking()
                        on se.ClassId equals c.ClassId
                    join sec in _context.Sections.AsNoTracking()
                        on se.SectionId equals sec.SectionId

                    where ps.ParentId == parentId
                          && s.IsActive
                          && se.IsActive

                    select new StudentDto
                    {
                        StudentID = s.StudentID,
                        StudentName = s.StudentName,
                        RollNumber = s.RollNumber,
                        ClassName = c.ClassName,
                        SectionName = sec.SectionName,
                        ProfileImagePath = s.ProfileImagePath
                    }
                ).ToListAsync();

                if (!students.Any())
                    return students;

                // -------------------------------
                // STEP 2: ATTENDANCE SUMMARY
                // -------------------------------

                var studentIds = await _context.ParentStudent
                .Where(ps => ps.ParentId == parentId)
                .Select(ps => ps.StudentId)
                .ToListAsync();

                // STEP 1: LOAD RAW ATTENDANCE DATA
                var attendanceRows = await (
                     from a in _context.Attendance
                     join ps in _context.ParentStudent
                         on a.StudentId equals ps.StudentId

                     where ps.ParentId == parentId

                     select a
                 ).AsNoTracking().ToListAsync();


                // STEP 2: GROUP IN MEMORY
                var attendanceDict = attendanceRows
                .GroupBy(a => a.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    Total = g.Count(),
                    Present = g.Count(x => x.Status == AttendanceStatus.Present)
                })
                .ToDictionary(x => x.StudentId);

                //var attendanceDict = await _context.Attendance
                //    .AsNoTracking()
                //    .Where(a => studentIds.Contains(a.StudentId))
                //    .GroupBy(a => a.StudentId)
                //    .Select(g => new
                //    {
                //        StudentId = g.Key,
                //        Total = g.Count(),
                //        Present = g.Count(x => x.Status == Enum.AttendanceStatus.Present)
                //    })
                //    .ToDictionaryAsync(x => x.StudentId);


                // -------------------------------
                // STEP 3: FEE SUMMARY
                // -------------------------------
                var feeDict = await (
                    from ps in _context.ParentStudent.AsNoTracking()
                    join sf in _context.StudentFees.AsNoTracking()
                        on ps.StudentId equals sf.StudentId

                    join fp in _context.FeePayments.AsNoTracking()
                        on sf.FeeId equals fp.FeeId into payments
                    from fp in payments.DefaultIfEmpty()

                    where ps.ParentId == parentId

                    group new { sf, fp } by ps.StudentId into g

                    select new
                    {
                        StudentId = g.Key,
                        TotalFee = g.Sum(x => x.sf.MonthlyAmount),
                        PaidAmount = g.Sum(x => x.fp != null ? x.fp.AmountPaid : 0)
                    }
                )
                .ToDictionaryAsync(x => x.StudentId);


                // -------------------------------
                // STEP 4: MERGE DATA (FINAL OUTPUT)
                // -------------------------------
                var result = new List<StudentDto>();
                foreach (var s in students)
                {
                    attendanceDict.TryGetValue(s.StudentID, out var att);
                    feeDict.TryGetValue(s.StudentID, out var fee);

                    var total = att?.Total ?? 0;
                    var present = att?.Present ?? 0;

                    var pending = fee == null ? 0 : fee.TotalFee - fee.PaidAmount;

                    result.Add(new StudentDto
                    {
                        StudentID = s.StudentID,
                        StudentName = s.StudentName,
                        RollNumber = s.RollNumber,
                        ClassName = s.ClassName,
                        SectionName = s.SectionName,
                        ProfileImagePath = s.ProfileImagePath,
                        AttendancePercentage =
                            total == 0 ? 0 : (present * 100.0m) / total,

                        PendingFees = pending,

                        FeeStatus =
                            fee == null ? "No Data" : (pending > 0 ? "Pending" : "Paid")
                    });
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<ParentProfileViewModel> BindParentProfileModelAsync()
        {
            // Logged-in parent id
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            int parentId = int.Parse(userIdClaim);

            // Parent Details
            var data = await _context.Parents
                .AsNoTracking()
                .Where(x => x.ParentId == parentId)
                .Select(x => new
                {
                    x.FatherName,
                    x.ParentEmail,
                    x.Phone,
                    x.Address
                })
                .FirstOrDefaultAsync();

            // Student Details
            var students = await GetParentDashboardAsync(parentId);

            // Bind Model
            var model = new ParentProfileViewModel
            {
                ParentId = parentId,
                ParentName = data?.FatherName,
                Email = data?.ParentEmail,
                MobileNumber = data?.Phone,
                Address = data?.Address,
                Students = students
            };

            return model;
        }

        public async Task<IActionResult> UpdateContactDetail(ParentProfileViewModel model)
        {
            // Email validation
            if (string.IsNullOrWhiteSpace(model.Email) ||
                !Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("Email", "Please enter a valid Email Address");
            }

            // Mobile validation
            if (string.IsNullOrWhiteSpace(model.MobileNumber) ||
                !Regex.IsMatch(model.MobileNumber, @"^[6-9]\d{9}$"))
            {
                ModelState.AddModelError("MobileNumber", "Please enter a valid Mobile Number");
            }

            if (!ModelState.IsValid)
            {
                model = await BindParentProfileModelAsync();
                return View("Profile", model); // IMPORTANT
            }

            var parent = await _context.Parents
                .FirstOrDefaultAsync(x => x.ParentId == model.ParentId);

            if (parent == null)
            {
                ModelState.AddModelError("", "Parent not found");
            }
            else
            {
                // Duplicate check
                if (_context.Parents.Any(x => x.ParentEmail == model.Email && x.ParentId != model.ParentId))
                {
                    ModelState.AddModelError("Email", "Email already exists");
                }

                if (_context.Parents.Any(x => x.Phone == model.MobileNumber && x.ParentId != model.ParentId))
                {
                    ModelState.AddModelError("MobileNumber", "Mobile number already exists");
                }

                if (!ModelState.IsValid)
                {
                    model = await BindParentProfileModelAsync();
                    return View("Profile", model); // IMPORTANT
                }

                parent.Address = model.Address;
                parent.Phone = model.MobileNumber;
                parent.ParentEmail = model.Email;

                await _context.SaveChangesAsync();

                return RedirectToAction("Profile");
            }

            model = await BindParentProfileModelAsync();
            return View("Profile", model);
        }



    }
}
