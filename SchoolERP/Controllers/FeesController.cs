using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;
using System.Security.Claims;

namespace SchoolERP.Controllers
{
    //[Authorize(Roles = "Admin, SuperAdmin, Teacher")]
    [Authorize]
    public class FeesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFeesService _feesService;

        public FeesController(AppDbContext context, IFeesService feesService)
        {
            _context = context;
            _feesService = feesService;
        }

        //public async Task<IActionResult> PendingFees()
        //{
        //    try
        //    {
        //        var teacherId = User.IsInRole("Teacher")
        //            ? int.Parse( User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
        //            : (int?)null;

        //        var currentYear = DateTime.Now.Year;

        //        var data = await (
        //                from sf in _context.StudentFees
        //                join se in _context.StudentEnrollments on sf.StudentId equals se.StudentId
        //                join c in _context.Classes on se.ClassId equals c.ClassId
        //                join cs in _context.ClassSections on new
        //                    {
        //                        se.ClassId, se.SectionId
        //                    }
        //                    equals new  { cs.ClassId, cs.SectionId }
        //                where sf.Status == "DUE"
        //                    && sf.RemainingAmount > 0
        //                    && sf.FeeYear == currentYear && se.IsActive
        //                    && (teacherId == null || cs.ClassTeacherId == teacherId)
        //                select new
        //                {
        //                    sf.FeeId,
        //                    sf.StudentId,
        //                    StudentName = sf.Student.StudentName,
        //                    ClassName = c.ClassName,
        //                    PendingAmount = sf.RemainingAmount
        //                }
        //            ).ToListAsync();

        //        var feeIds = data.Select(x => x.FeeId).ToList();

        //        var paymentRows = (await _context.FeePayments.ToListAsync())
        //                        .Where(x => feeIds.Contains(x.FeeId)).ToList();

        //        // Load active students + class
        //        var classStudents = await (
        //                from se in _context.StudentEnrollments
        //                join c in _context.Classes on se.ClassId equals c.ClassId
        //                where se.IsActive
        //                select new
        //                {
        //                    se.StudentId,
        //                    se.ClassId,
        //                    c.ClassName
        //                }
        //            ).ToListAsync();

        //        // Load pending fees
        //        var pendingFees = await _context.StudentFees
        //            .Where(x => x.Status == "DUE" && x.RemainingAmount > 0)
        //            .Select(x => new
        //            {
        //                x.StudentId,
        //                x.RemainingAmount
        //            }).ToListAsync();

        //        var paymentData = paymentRows.GroupBy(x => x.FeeId)
        //            .Select(g => new
        //                {
        //                    FeeId = g.Key,
        //                    DueDate = g.OrderByDescending(x => x.InstallmentNo)
        //                        .FirstOrDefault()
        //                        ?.DueDate
        //                }
        //            ).ToList();

        //        var dueDates = paymentRows
        //            .GroupBy(x => x.FeeId)
        //            .Select(g => new
        //                {
        //                    FeeId = g.Key,
        //                    DueDate = g.OrderByDescending(x => x.InstallmentNo).First().DueDate
        //                }
        //            ).ToList();

        //        var students = data.Select(x =>
        //        {
        //            var payment = paymentData.FirstOrDefault(p => p.FeeId == x.FeeId);

        //            return new StudentPendingFeeDto
        //            {
        //                StudentId = x.StudentId ?? 0,
        //                StudentName = x.StudentName,
        //                ClassName = x.ClassName,
        //                PendingAmount = x.PendingAmount,
        //                DueDate = payment?.DueDate ?? DateTime.MinValue,
        //                IsOverdue = payment != null && payment.DueDate < DateTime.Today
        //            };
        //        }).ToList();

        //        var classSummary = classStudents.GroupBy
        //            (x => new
        //                { x.ClassId, x.ClassName })
        //            .Select
        //            (
        //                g => 
        //                {
        //                    var studentIds = g.Select(x => x.StudentId)
        //                    .Distinct()
        //                    .ToList();

        //                    var totalPending = pendingFees
        //                        .Where(f => studentIds.Contains( f.StudentId ?? 0))
        //                        .Sum(f => f.RemainingAmount);

        //                    return new ClassPendingFeeDto
        //                    {
        //                        ClassName = g.Key.ClassName,

        //                        // TOTAL students in class
        //                        PendingStudents = studentIds.Count,

        //                        // TOTAL pending of class
        //                        PendingAmount = totalPending
        //                    };
        //                }
        //            ).OrderByDescending( x => x.PendingAmount)
        //            .ToList();

        //        // Calculate %
        //        var grandTotal = classSummary.Sum( x => x.PendingAmount);

        //        foreach (var item in classSummary)
        //        {
        //            item.PendingPercentage = grandTotal == 0 ? 0 :
        //                (int) Math.Round(item.PendingAmount * 100m / grandTotal);
        //        }

        //        // Percentage
        //        var totalPending = classSummary.Sum(x => x.PendingAmount);

        //        var vm = new PendingFeeDashboardVM
        //            {
        //                TotalPending = students.Sum(x => x.PendingAmount),
        //                TotalStudents = students.Count(),
        //                TotalClasses = students
        //                    .Select(x => x.StudentId)
        //                    .Distinct().Count(),
        //                OverdueCount = students.Count( x => x.IsOverdue),
        //                ClassWisePending = classSummary,
        //                PendingStudents = students
        //            };

        //        return View(vm);
        //    }
        //    catch (Exception)
        //    {
        //        return View(new PendingFeeDashboardVM());
        //    }
        //}

        [Authorize(Roles = "Admin, SuperAdmin, Teacher")]
        public async Task<IActionResult> PendingFees(int? classId)
        {
            var currentYear = DateTime.Now.Year;
            int? teacherId = null;
            if (User.IsInRole("Teacher"))
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(claim, out var id))
                {
                    int userId = id;
                    teacherId = await _context.Teachers
                       .Where(x => x.UserID == userId)
                       .Select(x => x.TeacherID)
                       .FirstOrDefaultAsync();
                }
            }
            else
            {
                ViewBag.Classes = await _context.Classes
                    .OrderBy(x => x.ClassOrder)
                    .ToListAsync();
            }

            // ==================================
            // Base Data
            // ==================================
            var data = await (
                from sf in _context.StudentFees
                join se in _context.StudentEnrollments
                    on sf.StudentId equals se.StudentId
                join c in _context.Classes
                    on se.ClassId equals c.ClassId
                join cs in _context.ClassSections
                    on new
                    {
                        se.ClassId,
                        se.SectionId
                    }
                    equals new
                    {
                        cs.ClassId,
                        cs.SectionId
                    }
                where
                    sf.Status == "DUE" &&
                    sf.RemainingAmount > 0 && sf.FeeYear == currentYear
                    && se.IsActive &&
                    (
                        teacherId == null ||  cs.ClassTeacherId == teacherId
                    )
                select new
                {
                    sf.FeeId,
                    StudentId = sf.StudentId ?? 0,
                    StudentName = sf.Student.StudentName,
                    AdmissionNo = sf.Student.AdmissionNumber,
                    MobileNo = sf.Student.PhoneNo,
                    ClassId =se.ClassId,
                    ClassName = c.ClassName,
                    ToatlFees = sf.TotalFee,
                    PendingAmount = sf.RemainingAmount
                }
            )
            .AsNoTracking()
            .ToListAsync();

            // ==================================
            // Fee Due Dates
            // ==================================
            var feeIds =
                data
                .Select(x => x.FeeId)
                .Distinct()
                .ToList();

            var dueDateLookup = (await _context.FeePayments.ToListAsync())
                .Where(x => feeIds.Contains(x.FeeId))
                .Select(x => new
                {
                    x.FeeId,
                    x.InstallmentNo,
                    x.DueDate
                })
                .ToList();

            var latestDueDates = dueDateLookup.GroupBy(x => x.FeeId)
                .ToDictionary( g =>  g.Key,
                    g => g.OrderByDescending(
                        x =>
                        x.InstallmentNo)
                    .First()
                    .DueDate
                );

            // ==================================
            // Student List
            // ==================================
            var students = data.Select(x =>
                {
                    latestDueDates.TryGetValue( x.FeeId, out var dueDate);
                    return new StudentPendingFeeDto
                    {
                        StudentId = x.StudentId,
                        StudentName = x.StudentName,
                        AdmissionNo = x.AdmissionNo,
                        MobileNo = x.MobileNo,
                        ClassName = x.ClassName,
                        ToatlFees = x.ToatlFees,
                        PendingAmount = x.PendingAmount,
                        DueDate = dueDate,
                        IsOverdue = dueDate < DateTime.Today
                    };
                })
                .ToList();

            // ==================================
            // Class Summary
            // ==================================
            var grandTotal = students.Sum(x => x.PendingAmount);

            var classSummary = data
                .GroupBy(x =>
                    new
                    {
                        x.ClassId,
                        x.ClassName
                    })

                .Select(g =>
                    new ClassPendingFeeDto
                    {
                        ClassName = g.Key.ClassName,
                        PendingStudents = g
                            .Select( x => x.StudentId)
                            .Distinct()
                            .Count(),
                        PendingAmount = g.Sum(x => x.PendingAmount),
                        PendingPercentage = grandTotal == 0 ? 0 :

                            (int) Math.Round(
                                g.Sum( x =>  x.PendingAmount)  *  100m / grandTotal
                            )
                    })
                .OrderByDescending( x => x.PendingAmount)
                .ToList();

            // ==================================
            // Dashboard
            // ==================================
            var vm =
                new PendingFeeDashboardVM
                {
                    TotalPending = students.Sum( x => x.PendingAmount),
                    TotalStudents = students
                        .Select( x => x.StudentId)
                        .Distinct()
                        .Count(),

                    TotalClasses = classSummary.Count(),
                    OverdueCount = students.Count(x => x.IsOverdue),
                    ClassWisePending = classSummary,
                    PendingStudents = students
                };

            ViewBag.ClassId = classId;

            return View(vm);
        }


        [Authorize(Roles = "Student, Parent")]
        [HttpGet]
        public async Task<IActionResult> Installments()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = int.Parse(userIdClaim); // get logged -in existingStudent id

            var studentId = _context.Students
                .Where(s => s.UserID == userId)
                .Select(s => s.StudentID)
                .FirstOrDefault();

            var dueDay = _context.FeeConfigurations.Select(x => x.DueDay).FirstOrDefault();
            var fees = await _feesService.GetStudentFeesAsync(studentId);
            ViewBag.DueDay = dueDay;
            return View(fees);
        }

        [Authorize(Roles = "Student, Parent")]
        [HttpGet]
        public async Task<IActionResult> Pay(int studentId, int month)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int userId = int.Parse(userIdClaim); // get logged -in existingStudent id
                FeePaymentViewModel feePaymentView = await _feesService.ReadFeePaymentAsync(userId, month);
                return View(feePaymentView);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return RedirectToAction("Installments");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetPaymentHistory(int FeeId)
        {
            var payments = await _context.FeePayments
                .AsNoTracking()
                .Where(x => x.FeeId == FeeId)
                .OrderByDescending(x => x.PaymentDate)
                .ToListAsync();

            return Json(payments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(FeePaymentViewModel model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = int.Parse(userIdClaim); // get logged -in existingStudent id

            FeePaymentViewModel feePayment = await _feesService.CalculateFeesAmount(userId, model.DueDate.Month);

            //    var payment = _context.Payments
            //        .FirstOrDefault(x => x.Id == model.PaymentId);

            //    if (payment == null)
            //        return NotFound();

            //    // IMPORTANT:
            //    // Recalculate on server

            //    var dueDate = CalculateDueDate(payment);

            //    var lateFine = CalculateLateFine(dueDate);

            //    var totalAmount =
            //        payment.MonthlyAmount + lateFine;

            //    // Create payment gateway order here

            //    // Example:
            //    // Razorpay / PhonePe order
            //    //return RedirectToAction("Success", new { id = transaction.Id });

            return RedirectToAction("Success");
        }

        [Authorize(Roles = "Student, Parent")]
        public IActionResult Success()
        {
            return View();
        }


    }
}
