using SchoolERP.Data;
using SchoolERP.Enum;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SchoolERP.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly AppDbContext _context;

        public AttendanceController(
            IAttendanceService attendanceService, AppDbContext context)
        {
            _attendanceService = attendanceService;
            _context = context;
        }

        [Authorize(Roles = "Teacher, Admin")]
        // GET: Attendance/Student
        public async Task<IActionResult> Student()
        {
            var teacherId = await GetLoggedInTeacherId();

            // Await the async call
            var classes = await _context.ClassSectionSubject
                .Where(x => x.TeacherId == teacherId)
                .Select(x => new
                {
                    x.ClassSection.Class.ClassId,
                    x.ClassSection.Class.ClassName
                })
                .Distinct()
                .ToListAsync();

            ViewBag.Classes = await _context.Classes
                .Select(y => new SelectListItem
                {
                    Value = y.ClassId.ToString(),
                    Text = y.ClassName
                }).ToListAsync();

            ViewBag.Subjects = await _context.Subjects
                .Select(y => new SelectListItem
                {
                    Value = y.SubjectId.ToString(),
                    Text = y.SubjectName
                }).ToListAsync();

            return View();
        }

        // Get Assigned section to class 
        public async Task<IActionResult> GetSections(int classId)
        {
            var teacherId = await GetLoggedInTeacherId();

            var sections = await _context.ClassSectionSubject
                .Where(x =>
                    x.TeacherId == teacherId &&
                    x.ClassSection.ClassId == classId)
                .Select(x => new
                {
                    x.ClassSection.Section.SectionId,
                    x.ClassSection.Section.SectionName,
                    x.ClassSectionId
                })
                .Distinct()
                .ToListAsync();

            return Json(sections);
        }

        // Get Assigned subject to the teacher 
        public async Task<IActionResult> GetSubjects(int classSectionId)
        {
            var teacherId = await GetLoggedInTeacherId();

            var subjects = await _context.ClassSectionSubject
                .Where(x =>
                    x.TeacherId == teacherId &&
                    x.ClassSectionId == classSectionId)
                .Select(x => new
                {
                    x.Subject.SubjectId,
                    x.Subject.SubjectName
                })
                .ToListAsync();

            return Json(subjects);
        }

        [HttpGet]
        public IActionResult GetClassDetails(int classId)
        {
            // Get all sections for this class
            var sections = _context.ClassSections
                .Where(cs => cs.ClassId == classId)
                .Select(cs => new
                {
                    cs.Id,             // ClassSectionId
                    SectionId = cs.SectionId,
                    SectionName = cs.Section.SectionName
                }).Distinct().ToList();
            //Because ClassSections may have duplicates or multiple mappings.
            return Json(new { sections });
        }

        //[HttpGet]
        //public IActionResult GetStudents( int classSectionId)
        //{
        //    var students = _context.Students
        //        .Where(s => s.ClassSectionId == classSectionId)
        //        .Select(s => new
        //        {
        //            id = s.StudentID,
        //            name = s.StudentName + " - " + s.RollNumber,
        //        })
        //        .ToList();

        //    return Json(students);
        //}

        public async Task<IActionResult> GetStudents(int classid, int sectionid, int classSectionId)
        {
            //var students = _context.Students
            //    .Where(s => s.StudentEnrollments.Any(se =>
            //        se.ClassId == classid &&
            //        se.SectionId == sectionid &&
            //        se.Status == "Active"
            //    ))
            //    .Select(s => new
            //    {
            //        id = s.StudentID,
                    
            //        name = s.StudentName + " - " + s.RollNumber
            //    })
            //    .ToList();

            var students = await _context.StudentEnrollments
                .Where(se =>
                    se.ClassId == classid &&
                    se.SectionId == sectionid &&
                    se.Status == "Active")
                .Select(se => new
                {
                    id = se.Student.StudentID,
                    rollno = se.RollNumber,
                    name = se.Student.StudentName 
                })
                .OrderBy(x => x.rollno)
                .ToListAsync();

            return Json(students);
        }


        [Authorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] AttendanceSaveRequest request)
        {
            if (request == null || request.Records == null || !request.Records.Any())
            {
                return Json(new { success = false, message = "Invalid attendance data." });
            }

            if (request.AttendanceDate > DateTime.Today)
            {
                return Json(new { success = false, message = "Future date not allowed." });
            }

            int teacherId = await GetLoggedInTeacherId();

            //// 🔐 Get logged-in teacher
            //int? teacherId =  await _context.Teachers
            //    .Where(t => t.UserID == userId)
            //    .Select(t => t.TeacherID)
            //    .FirstOrDefaultAsync();

            var exists = _context.Attendance
                .Any(a => a.SubjectID == request.SubjectId &&
                          a.AttendanceDate == request.AttendanceDate);
            if (exists)
            {
                return Json(new { success = false, message = "\"Attendance already marked for this date" });
                //return BadRequest("Attendance already marked for this date");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                //var studentIds = request.Records.Keys
                //.Select(id => (int?)id)
                //.ToList();

                var studentIds = request.Records.Keys.ToList();
                var date = request.AttendanceDate.Date;

                // Step 1: simple SQL (NO Contains)
                var query = _context.Attendance
                    .Where(x =>
                        x.SubjectID == request.SubjectId &&
                        x.AttendanceDate == date);

                var data = await query.ToListAsync();

                // 🔥 Fetch existing records to prevent duplicates
                // Step 2: in-memory filtering
                var existingRecords = data
                    .Where(x => studentIds.Contains(x.StudentId))
                    .ToList();

                var existingMap = existingRecords.ToDictionary(x => x.StudentId);

                var newRecords = new List<Attendance>();

                foreach (var item in request.Records)
                {
                    int studentId = item.Key;
                    string statusStr = item.Value;

                    // 🔥 Convert string → enum safely
                    if (!System.Enum.TryParse<AttendanceStatus>(statusStr, true, out var status))
                        continue;

                    // ✅ Validate status
                    if (status != AttendanceStatus.Present && status != AttendanceStatus.Absent)
                        continue;

                    if (existingMap.ContainsKey(studentId))
                    {
                        // 🔁 Update existing
                        var existing = existingMap[studentId];
                        existing.Status = status;
                        existing.TeacherID = teacherId;
                    }
                    else
                    {
                        // ➕ Insert new
                        newRecords.Add(new Attendance
                        {
                            StudentId = studentId,
                            SubjectID = request.SubjectId,
                            AttendanceDate = request.AttendanceDate,
                            Status = status,
                            TeacherID = teacherId
                        });
                    }
                }

                if (newRecords.Any())
                    await _context.Attendance.AddRangeAsync(newRecords);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "Attendance saved successfully"

                    //,present = request.Records.Count(x => x.Value == "Present"),
                    //absent = request.Records.Count(x => x.Value == "Absent")
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // ⚠️ Log error properly (Serilog / NLog)
                //return StatusCode(500, new
                //{
                //    success = false,
                //    message = "Something went wrong",
                //    error = ex.Message
                //});
                return Json(new { success = false, message = "Something went wrong." });
            }
        }


        // GET: Attendance/Teacher
        public IActionResult Teacher()
        {
            // Load Teacher attendance data
            return View();
        }

        //Get the Logged-In Techer TeacherID
        protected async Task<int> GetLoggedInTeacherId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim == null)
                throw new Exception("Sorry, you’re not authorized to access this page.");

            // convert to int if needed
            int userId = int.Parse(claim);

            // 🔐 Get logged-in teacher
            int? teacherId = await _context.Teachers
                .Where(t => t.UserID == userId)
                .Select(t => t.TeacherID)
                .FirstOrDefaultAsync();

            return teacherId.Value;
        }

        public async Task<IActionResult> MyAttendance(string view = "month")
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim == null)
                throw new Exception("Sorry, you’re not authorized to access this page.");

            // convert to int if needed
            int userId = int.Parse(claim);

            int studentId = await _context.Students
                .Where(s => s.UserID == userId)
                .Select(s => s.StudentID)
                .FirstOrDefaultAsync();

            var today = DateTime.Today;

            DateTime startDate;
            DateTime endDate;
            // ✅ Decide date range
            if (view == "week")
            {
                startDate = today.AddDays(-7);   // last 7 days
                endDate = today;
            }
            else // month
            {
                startDate = new DateTime(today.Year, today.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1); // last day of month
            }

            // ✅ Fetch ONLY required filtered data
            var data = await _context.Attendance
                .Where(x => x.StudentId == studentId &&
                            x.AttendanceDate >= startDate &&
                            x.AttendanceDate < endDate.AddDays(1)) // safer for time
                .Select(x => new
                {
                    x.AttendanceDate,
                    x.Status,
                    Subject = x.Subject.SubjectName
                })
                .ToListAsync();

            // ✅ Overall calculations (filtered dataset)
            var total = data.Count;
            var present = data.Count(x => x.Status == AttendanceStatus.Present);
            var filtered = data.Where(x => x.AttendanceDate >= startDate).ToList();

            decimal percentage = total == 0 ? 0 : (present * 100m / total);

            // ✅ Separate week calculation (independent of view)
            //var weekStart = today.AddDays(-7);

            // 👇 take the later date
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var weekStart = today.AddDays(-7) < monthStart
                            ? monthStart
                            : today.AddDays(-7);
            var weekEnd = today;

            var weekData = await _context.Attendance
               .Where(x => x.StudentId == studentId &&
                           x.AttendanceDate >= weekStart &&
                           x.AttendanceDate < today.AddDays(1))
               .ToListAsync();

            var weekTotal = weekData.Count;
            var weekPresent = weekData.Count(x => x.Status == AttendanceStatus.Present);
            decimal weekPercentage = weekTotal == 0 ? 0 : (weekPresent * 100m / weekTotal);

            // ✅ Month calculation (independent of view)
            //var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthData = await _context.Attendance
                .Where(x => x.StudentId == studentId &&
                            x.AttendanceDate >= monthStart &&
                            x.AttendanceDate < monthEnd.AddDays(1))
                .ToListAsync();

            var monthTotal = monthData.Count;
            var monthPresent = monthData.Count(x => x.Status == AttendanceStatus.Present);

            decimal monthPercentage = monthTotal == 0 ? 0 : (monthPresent * 100m / monthTotal);

            var vm = new StudentAttendanceVM
            {
                OverallPercentage = Math.Round(percentage, 2),
                WeekPercentage = Math.Round(weekPercentage, 2),
                MonthPercentage = Math.Round(monthPercentage, 2),

                TotalPresent = present,
                TotalAbsent = total - present,

                // ✅ Latest records (filtered)
                AttendanceList = data
                    .OrderByDescending(x => x.AttendanceDate)
                    .Take(10)
                    .Select(x => new AttendanceDto
                    {
                        Date = x.AttendanceDate,
                        Subject = x.Subject,
                        Status = x.Status
                    })
                    .ToList(),

                // ✅ Subject-wise stats (filtered)
                SubjectStats = data
                    .GroupBy(x => x.Subject)
                    .Select(g => new SubjectAttendanceDto
                    {
                        Subject = g.Key,
                        Present = g.Count(x => x.Status == AttendanceStatus.Present),
                        Absent = g.Count(x => x.Status == AttendanceStatus.Absent),
                        Percentage = Math.Round(
                            g.Count(x => x.Status == AttendanceStatus.Present) * 100m / g.Count(), 2)
                    })
                    .ToList()
            };

            return View(vm);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AttendanceReport(int? classId, DateTime? attendanceDate)
        {
            ViewBag.ClassId = classId;
            var date = attendanceDate ?? DateTime.Today;

            ViewBag.Classes = await _context.Classes
            .OrderBy(x => x.ClassName)
            .ToListAsync();

            var report = await (
                from se in _context.StudentEnrollments
                join c in _context.Classes
                    on se.ClassId equals c.ClassId
                join s in _context.Sections
                    on se.SectionId equals s.SectionId
                group se by new
                {
                    se.ClassId,
                    se.SectionId,
                    c.ClassName,
                    s.SectionName
                }
                into g
                select new ClassAttendanceReportDto
                {
                    ClassId = g.Key.ClassId,
                    SectionId = g.Key.SectionId,
                    ClassName = g.Key.ClassName,
                    SectionName = g.Key.SectionName,
                    Strength = g.Count()
                }
            ).ToListAsync();

            foreach (var item in report)
            {
                item.AttendancePercentage =
                    item.Strength == 0
                        ? 0
                        : Math.Round(
                            item.PresentCount * 100m /
                            item.Strength,
                            1);

                item.Status =
                    item.AttendancePercentage >= 95 ? "Excellent" :
                    item.AttendancePercentage >= 90 ? "Good" :
                    item.AttendancePercentage >= 80 ? "Average" :
                    "Low";
            }

            if (classId.HasValue)
            {
                report = report
                    .Where(x => x.ClassId == classId)
                    .ToList();
            }

            var vm = new AttendanceDashboardDto
            {
                AttendanceDate = date,
                TotalStudents = report.Sum(x => x.Strength),
                PresentStudents = report.Sum(x => x.PresentCount),
                AbsentStudents = report.Sum(x => x.AbsentCount),
                AttendancePercentage =
                    report.Sum(x => x.Strength) == 0
                        ? 0
                        : Math.Round(
                            report.Sum(x => x.PresentCount) * 100m /
                            report.Sum(x => x.Strength),
                            1),

                Classes = report
            };

            return View(vm);
        }

        public async Task<IActionResult> GetAttendanceDetails1(int classId, DateTime attendanceDate)
        {
            var students = await (
                from a in _context.Attendance
                join se in _context.StudentEnrollments
                    on a.StudentId equals se.StudentId
                join s in _context.Students
                on se.StudentId equals s.StudentID
                where a.AttendanceDate == attendanceDate
                      && se.ClassId == classId

                select new StudentAttendanceDto
                {
                    StudentName = s.StudentName,
                    RollNo = se.RollNumber,
                    Status = a.Status
                }

            ).ToListAsync();

            return Json(students);
        }

        public async Task<IActionResult> GetAttendanceDetails(int classId, int sectionId, DateTime attendanceDate)
        {
            var attendanceExists = await _context.Attendance
                .AnyAsync(x =>
                    x.AttendanceDate.Date == attendanceDate.Date);

            if (!attendanceExists)
            {
                var students = await (
                    from se in _context.StudentEnrollments
                    join s in _context.Students
                        on se.StudentId equals s.StudentID
                    where se.ClassId == classId
                          && se.SectionId == sectionId
                    select new StudentAttendanceDto
                    {
                        StudentName = s.StudentName,
                        RollNo = se.RollNumber,
                        Status = AttendanceStatus.Pending
                    }
                ).ToListAsync();

                return Json(students);
            }
            else
            {
                var students = await (
                    from se in _context.StudentEnrollments
                    join s in _context.Students
                        on se.StudentId equals s.StudentID
                    join a in _context.Attendance
                        on s.StudentID equals a.StudentId
                    where se.ClassId == classId
                          && se.SectionId == sectionId
                          && a.AttendanceDate.Date == attendanceDate.Date

                    select new StudentAttendanceDto
                    {
                        StudentName = s.StudentName,
                        RollNo = se.RollNumber,
                        Status = AttendanceStatus.Absent
                    }
                )
                .OrderBy(x => x.RollNo)
                .ToListAsync();

                return Json(students);
            }

                
        }



    }
}
