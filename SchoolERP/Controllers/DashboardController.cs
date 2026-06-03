
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SchoolERP.Data;
using SchoolERP.Enum;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using System.Security.Claims;

namespace SchoolERP.Controllers
{
    [Authorize]
    //[NoCache]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public DashboardController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }


        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var role = User.FindFirstValue(ClaimTypes.Role)?.ToLower();

            bool isStudent = (role == "student");
            bool isAdmin = (role == "admin" || role == "superadmin");

            // =========================
            // DATE RANGES
            // =========================

            var currentMonthStart = new DateTime(now.Year, now.Month, 1);

            var nextMonthStart = currentMonthStart.AddMonths(1);

            var previousMonthStart = currentMonthStart.AddMonths(-1);

            var startOfYear = new DateTime(now.Year, 1, 1);

            // =========================
            // VIEW MODEL
            // =========================
            var model = new DashboardViewModel();

            model.TotalHolidays =
                await _context.Holidays
                    .AsNoTracking()
                    .CountAsync();

            model.UpcomingExamCount =
                await _context.ExamSchedules
                    .AsNoTracking()
                    .CountAsync(x => x.ExamDate >= now);

            // =========================
            // ROLE BASED DASHBOARD
            // =========================
            if (isStudent)
            {
                await LoadStudentDashboard(
                    model,
                    userId,
                    now,
                    currentMonthStart,
                    nextMonthStart);
            }
            else
            {
                // =========================
                // COMMON DATA
                // =========================
                model.TeacherCount =
                    await _context.Teachers
                        .AsNoTracking()
                        .CountAsync();

                model.ClassCount =
                    await _context.Classes
                        .AsNoTracking()
                        .CountAsync();

                model.SubjectCount =
                    await _context.Subjects
                        .AsNoTracking()
                        .CountAsync();

                await LoadAdminDashboard(
                    model,
                    now,
                    startOfYear,
                    currentMonthStart,
                    previousMonthStart,
                    nextMonthStart);

                model.UpcomingTeacherBirthDay = await GetUpcommingTechBirthdaysAsync(userId, now);
            }

            // =========================
            // NOTIFICATIONS
            // =========================
            model.Notifications = await GetNotificationsAsync(userId, role, now);
            model.UpcomingBirthdays = await GetUpcomingBirthdaysAsync(userId, now);
            return View(model);
        }

        private async Task LoadStudentDashboard(
            DashboardViewModel model, int userId, DateTime now,
            DateTime currentMonthStart, DateTime nextMonthStart)
        {
            int studentId = await _context.Students
                .Where(x => x.UserID == userId)
                .Select(x => x.StudentID)
                .FirstOrDefaultAsync();

            // Pending Fees
            var dueDay = await _context.FeeConfigurations
                .Select(x => x.DueDay)
                .FirstOrDefaultAsync();

            var dueDate = new DateTime(now.Year, now.Month, dueDay);
            decimal pendingFees = 0;
            //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (User.IsInRole("Student"))
            {
                pendingFees = await _context.StudentFees
                .Where(x => x.StudentId == studentId && x.RemainingAmount > 0)
                .SumAsync(x => x.RemainingAmount);

                model.PendingFees = pendingFees;
            }

            // Attendance
            var attendance =
                await _context.Attendance
                    .Where(x =>
                        x.StudentId == studentId &&
                        x.AttendanceDate >= currentMonthStart &&
                        x.AttendanceDate < nextMonthStart)
                    .ToListAsync();

            var total = attendance.Count;
            var present = attendance.Count(x => x.Status == AttendanceStatus.Present);
            model.AttendancePercentage = total == 0 ? 0 : Math.Round((decimal)present / total * 100, 2);
        }

        private async Task LoadAdminDashboard(DashboardViewModel model, DateTime now,
            DateTime startOfYear, DateTime currentMonthStart, DateTime previousMonthStart, DateTime nextMonthStart)
        {
            var students = await _context.Students
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .Select(x => new
                    {
                        x.CreatedAt
                    })
                    .ToListAsync();

            model.StudentCount = students.Count;
            model.NewStudentCount = students.Count(x => x.CreatedAt >= startOfYear);

            model.NewAdmissionsThisMonth = students.Count(x => x.CreatedAt >= currentMonthStart);

            //Get DueDay First (from FeeConfigurations)
            var dueDay = _context.FeeConfigurations.Select(x => x.DueDay).FirstOrDefault();
            var dueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, dueDay);

            decimal pendingFees = 0;
            if (User.IsInRole("Teacher"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                int teacherId = await _context.Teachers
                   .Where(x => x.UserID == userId)
                   .Select(x => x.TeacherID)
                   .FirstOrDefaultAsync();

                pendingFees = await
                (
                    from sf in _context.StudentFees
                    join se in _context.StudentEnrollments
                        on sf.StudentId equals se.StudentId
                    join cs in _context.ClassSections
                        on se.ClassId equals cs.ClassId
                    where cs.ClassTeacherId == teacherId
                          && sf.RemainingAmount > 0
                    select sf.RemainingAmount
                ).SumAsync();
            }
            else
            {
                pendingFees = await _context.StudentFees
                    .Where(x => x.RemainingAmount > 0)
                    .SumAsync(x => x.RemainingAmount);
            }
            model.PendingFees = pendingFees;

            // Chart
            var chartData =
                students
                    .GroupBy(x => new
                    {
                        x.CreatedAt.Year,
                        x.CreatedAt.Month
                    })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToList();

            // =========================
            // ATTENDANCE (monthly)
            // =========================
            var attendance = await _context.Attendance
                .AsNoTracking()
                .Where(x =>
                    x.AttendanceDate >= currentMonthStart &&
                    x.AttendanceDate < nextMonthStart)
                .Select(x => new { x.Status }).ToListAsync();
            var totalAttendance = attendance.Count;
            var presentAttendance = attendance.Count(x => x.Status == AttendanceStatus.Present);
            var attendancePercentage = totalAttendance == 0
                ? 0
                : Math.Round((decimal)presentAttendance / totalAttendance * 100, 2);
            model.AttendancePercentage = attendancePercentage;

            model.ChartLabels =
                chartData
                    .Select(x =>
                        new DateTime(
                            x.Year,
                            x.Month,
                            1).ToString("MMM"))
                    .ToList();

            model.ChartStudentData = chartData.Select(x => x.Count).ToList();
        }

        private async Task<List<DashboardNotificationVM>> GetNotificationsAsync(int userId,
            string role, DateTime now)
        {
            bool isAdmin = (role == "admin" || role == "superadmin");

            IQueryable<Notification> query =
                _context.Notifications.AsNoTracking()
                    .Where(x =>
                        x.IsActive &&
                        x.StartDate <= now &&
                        x.EndDate >= now);

            var notifications =
                await (
                    from n in query
                    join rs in _context.NotificationReadStatus
                        .Where(x => isAdmin || x.UserId == userId)

                    on n.NotificationId equals rs.NotificationId
                    into readJoin

                    from readStatus in readJoin.DefaultIfEmpty()
                    orderby n.CreatedAt descending

                    select new DashboardNotificationVM
                    {
                        NotificationId = n.NotificationId,
                        Title = n.Title,
                        Message = n.Message,
                        Priority = n.Priority,

                        IsRead = readStatus != null && readStatus.IsRead,

                        TimeAgo =
                            EF.Functions.DateDiffMinute(
                                n.CreatedAt,
                                now) < 60

                            ? EF.Functions.DateDiffMinute(
                                n.CreatedAt,
                                now) + " mins ago"

                            : EF.Functions.DateDiffHour(
                                n.CreatedAt,
                                now) + " hrs ago",

                        Icon =
                            n.Priority == "High"
                                ? "bi bi-exclamation-circle"
                                : n.Priority == "Medium"
                                    ? "bi bi-bell"
                                    : "bi bi-info-circle",

                        ColorClass =
                            n.Priority == "High"
                                ? "danger"
                                : n.Priority == "Medium"
                                    ? "warning"
                                    : "primary"
                    })
                    .Take(10)
                    .ToListAsync();

            return notifications;
        }


        private async Task<List<UpcomingBirthdayDTO>> GetUpcomingBirthdaysAsync(int userId, DateTime today)
        {
            var isTeacher = User.IsInRole("Teacher");

            // =========================
            // BASE QUERY
            // =========================
            var query =
                (from s in _context.Students
                 join se in _context.StudentEnrollments on s.StudentID equals se.StudentId
                 join c in _context.Classes on se.ClassId equals c.ClassId
                 join sec in _context.Sections on se.SectionId equals sec.SectionId
                 join g in _context.Genders on s.GenderID equals g.GenderID
                 where s.IsActive
                 select new UpcomingBirthdayDTO
                 {
                     StudentId = s.StudentID,
                     StudentName = s.StudentName,
                     Gender = g.GenderName.ToLower(),
                     ClassId = c.ClassId,
                     ClassName = c.ClassName + " - " + sec.SectionName,
                     DOB = s.DateOfBirth.Value
                 });


            // =========================
            // TEACHER FILTER
            // =========================
            if (isTeacher)
            {
                int teacherId = await _context.Teachers
                   .Where(x => x.UserID == userId)
                   .Select(x => x.TeacherID)
                   .FirstOrDefaultAsync();

                //var teacherClassIds = await _context.ClassSections
                //    .Where(x => x.ClassTeacherId == teacherId)
                //    .Select(x => x.ClassId)
                //    .Distinct()
                //    .ToListAsync();

                query = query.Where(t => _context.ClassSections.Any(cs => cs.ClassTeacherId == teacherId));
            }

            // =========================
            // FETCH DATA
            // =========================
            var data = await query.ToListAsync();

            // =========================
            // UPCOMING DATE CALCULATION
            // =========================
            foreach (var item in data)
            {
                // Birthday in current year
                var nextBirthday = new DateTime(
                    today.Year,
                    item.DOB.Month,
                    item.DOB.Day);

                // If already passed this year
                if (nextBirthday < today)
                {
                    nextBirthday = nextBirthday.AddYears(1);
                }

                item.UpcomingDate = nextBirthday;

                item.DaysRemaining = (nextBirthday - today).Days;
            }

            // =========================
            // SHOW ONLY TODAY + UPCOMING
            // NEXT 30 DAYS
            // =========================
            var upcoming = data
                // Only upcoming birthdays
                .Where(x => x.DaysRemaining >= 0 && x.DaysRemaining <= 30)
                .OrderBy(x => x.DaysRemaining)
                // Then alphabetical
                .ThenBy(x => x.StudentName)
                .Take(4)
                .ToList();

            return upcoming;
        }

        private async Task<List<TeacherBirthdayDTO>> GetUpcommingTechBirthdaysAsync(int userId, DateTime today)
        {
            var isTeacher = User.IsInRole("Teacher");

            // =========================
            // BASE QUERY
            // =========================
            var query =
                from t in _context.Teachers
                join g in _context.Genders
                    on t.GenderID equals g.GenderID
                where t.IsActive
                      && t.DateOfBirth.HasValue
                select new
                {
                    t.TeacherID,
                    TeacherName = t.FullName,
                    Gender = g.GenderName,
                    Designation = t.Designation,
                    DOB = t.DateOfBirth.Value
                };

            // =========================
            // FETCH
            // =========================
            var teachers = await query
                .AsNoTracking()
                .OrderBy(x => x.TeacherName)
                .ToListAsync();

            // =========================
            // DTO MAPPING
            // =========================
            var data = teachers
                .Select(t => new TeacherBirthdayDTO
                {
                    TeacherId = t.TeacherID,
                    TeacherName = t.TeacherName,
                    Gender = t.Gender,
                    DOB = t.DOB,
                    Designation = t.Designation
                })
                .ToList();

            // =========================
            // UPCOMING CALCULATION
            // =========================
            foreach (var item in data)
            {
                var nextBirthday = new DateTime(
                    today.Year,
                    item.DOB.Month,
                    item.DOB.Day);

                if (nextBirthday < today)
                {
                    nextBirthday = nextBirthday.AddYears(1);
                }
                item.UpcomingDate = nextBirthday;
                item.DaysRemaining = (nextBirthday - today).Days;
            }

            // =========================
            // NEXT 30 DAYS ONLY
            // =========================
            var upcoming = data
                .Where(x =>
                    x.DaysRemaining >= 0
                    && x.DaysRemaining <= 30
                    )
                .OrderBy(x => x.DaysRemaining)
                .ThenBy(x => x.TeacherName)
                .Take(4)
                .ToList();

            return upcoming;
        }

        public ActionResult Dashboard()
        {
            return View();
        }

        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> Parents()
        {
            var userName = User.Identity.Name;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                // user is not logged in or claim missing
                return Unauthorized();
            }

            // convert to int
            int parentId = int.Parse(userIdClaim);

            try
            {
                #region Old Code 

                // var studentIds = await _context.ParentStudent
                //     .Where(ps => ps.ParentId == parentId)
                //     .Select(ps => ps.StudentId)
                //     .ToListAsync();

                //// var attendanceLookup = await _context.Attendance
                ////.GroupBy(a => a.StudentId)
                ////.Select(g => new
                ////{
                ////    StudentId = g.Key,
                ////    Total = g.Count(),
                ////    Present = g.Count(x => x.Status == Enum.AttendanceStatus.Present)
                ////})
                ////.ToListAsync();

                //// var attendanceDict = attendanceLookup
                ////         .ToDictionary(x => x.StudentId);

                // var attendanceDict = await _context.Attendance
                //     .AsNoTracking()
                //     .Where(a => studentIds.Contains(a.StudentId))
                //     .GroupBy(a => a.StudentId)
                //     .Select(g => new
                //     {
                //         StudentId = g.Key,
                //         Total = g.Count(),
                //         Present = g.Count(x => x.Status == Enum.AttendanceStatus.Present)
                //     })
                //     .ToDictionaryAsync(x => x.StudentId);


                // var feeData = await (
                //         from ps in _context.ParentStudent
                //         join sf in _context.StudentFees
                //             on ps.StudentId equals sf.StudentId

                //         where ps.ParentId == parentId

                //         join fp in _context.FeePayments
                //             on sf.FeeId equals fp.FeeId into payments
                //         from fp in payments.DefaultIfEmpty()

                //         group fp by ps.StudentId into g

                //         select new
                //         {
                //             StudentId = g.Key,
                //             TotalFee = g.Sum(x => x != null ? x.Amount : 0),
                //             PaidAmount = g.Sum(x => x != null ? x.AmountPaid : 0)
                //         }
                //     ).ToListAsync();

                // var feeDict = feeData.ToDictionary(x => x.StudentId);


                // var students = await (
                //     from p in _context.Parents

                //     join ps in _context.ParentStudent.AsNoTracking()
                //         on p.ParentId equals ps.ParentId

                //     join s in _context.Students
                //         on ps.StudentId equals s.StudentID

                //     join se in _context.StudentEnrollments
                //         on s.StudentID equals se.StudentId

                //     join c in _context.Classes
                //         on se.ClassId equals c.ClassId

                //     join sec in _context.Sections
                //         on se.SectionId equals sec.SectionId

                //     where p.ParentId == parentId
                //           && s.IsActive
                //           && se.IsActive

                //     select new StudentDto
                //     {
                //         StudentID = s.StudentID,
                //         StudentName = s.StudentName,
                //         RollNumber = s.RollNumber,
                //         ClassName = c.ClassName,
                //         SectionName = sec.SectionName,

                //         AttendancePercentage =
                //             attendanceDict.ContainsKey(s.StudentID)
                //             ? (attendanceDict[s.StudentID].Present * 100.0m)
                //               / attendanceDict[s.StudentID].Total
                //             : 0
                //     }
                // ).ToListAsync();

                #endregion

                List<StudentDto> studentDtos = await GetParentDashboardAsync(parentId);

                int roleId = await _context.Roles
                   .Where(r => r.RoleName.ToLower() == "parent")
                   .Select(r => r.RoleID)
                   .FirstOrDefaultAsync();

                // Todo Read the Notification 
                var today = DateTime.Today;
                var data = await (from n in _context.Notifications
                                  join t in _context.NotificationTargets
                                      on n.NotificationId equals t.NotificationId
                                  join r in _context.NotificationReadStatus
                                      on new { n.NotificationId, UserId = parentId }
                                      equals new { r.NotificationId, r.UserId }
                                      into readGroup
                                  from r in readGroup.DefaultIfEmpty()

                                  where t.RoleId == roleId
                                        && n.IsActive
                                        && n.EndDate >= today   // 🔥 KEY FIX

                                  orderby n.StartDate ascending, n.Priority descending

                                  select new NotificationDTO
                                  {
                                      NotificationId = n.NotificationId,
                                      Title = n.Title,
                                      Message = n.Message,
                                      StartDate = n.StartDate,
                                      EndDate = n.EndDate,
                                      Priority = n.Priority,
                                      IsRead = r != null && r.IsRead
                                  })
                              .Distinct()
                              .ToListAsync();


                var model = new ParentDashboardViewModel
                {
                    Students = studentDtos
                    , Notifications = data
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return View(new ParentDashboardViewModel());
            }
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
                    SectionName = sec.SectionName
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

    }
}
