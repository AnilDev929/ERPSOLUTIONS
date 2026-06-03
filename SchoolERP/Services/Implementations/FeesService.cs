using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Enum;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;

namespace SchoolERP.Services.Implementations
{
    public class FeesService : IFeesService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StudentProfileService> _logger;

        public FeesService(AppDbContext context, ILogger<StudentProfileService> logger) {

            _context = context;
            _logger = logger;
        }

        private async Task<(bool IsExempted, string Reason)> GetMonthExemption(DateTime installmentDate)
        {
            var exemption = await _context.AcademicCalendarExceptions
                .FirstOrDefaultAsync(x =>
                    x.IsBillingBlocked
                    && (installmentDate.Date >= x.StartDate.Date
                    && installmentDate.Date <= x.EndDate.Date)
                );

            return (exemption != null, exemption?.Description);
        }

        //private async Task<bool> IsMonthExempted(DateTime installmentDate)
        //{
        //    return await _context.AcademicCalendarExceptions
        //        .AnyAsync(x =>
        //            x.IsBillingBlocked &&
        //            installmentDate.Date >= x.StartDate.Date
        //            && installmentDate.Date <= x.EndDate.Date
        //        );
        //}

        //private async Task<string> GetMonthExemptionReason( DateTime installmentDate)
        //{
        //    return await _context.AcademicCalendarExceptions
        //        .Where(x =>
        //            x.IsBillingBlocked
        //            && installmentDate.Date >= x.StartDate.Date
        //            && installmentDate.Date <= x.EndDate.Date
        //        )
        //        .Select(x => x.Description)
        //        .FirstOrDefaultAsync();
        //}

        public async Task<StudentFeeDetailViewModel> GetStudentFeesAsync(int studentId)
        {
            try
            {
                // 1. Get latest fee (better: filter by AcademicYear if needed)
                var fee = await _context.StudentFees
                    .Include(sf => sf.AcademicYear)
                    .Where(stu => stu.StudentId == studentId)
                    .OrderByDescending(stu => stu.FeeId)
                    .FirstOrDefaultAsync();

                if (fee == null)
                    return new StudentFeeDetailViewModel();

                // 2. Student Info
                var student = await (
                    from s in _context.Students
                    join se in _context.StudentEnrollments on s.StudentID equals se.StudentId
                    //join cs in _context.ClassSections on s.ClassSectionId equals cs.Id
                    join c in _context.Classes on se.ClassId equals c.ClassId
                    join sec in _context.Sections on se.SectionId equals sec.SectionId
                    join ay in _context.AcademicYears on s.AcademicYearID equals ay.AcademicYearID
                    where s.StudentID == studentId && se.Status == "Active"
                    select new StudentInfoViewModel
                    {
                        StudentId = s.StudentID,
                        StudentName = s.StudentName,
                        Class = c.ClassName,
                        Section = sec.SectionName,
                        AdmissionNumber = s.AdmissionNumber
                    }).FirstOrDefaultAsync();

                // 3. Payment History
                int academicYearId = fee.AcademicYearID;
                var studentIdParam = new SqlParameter("@StudentId", studentId);
                var yearParam = new SqlParameter("@AcademicYearId", academicYearId);
                var result = await _context.Set<StudentFeeDto>()
                    .FromSqlRaw("EXEC dbo.GetStudentFeeMonthlyView_02 @StudentId, @AcademicYearId",
                        studentIdParam, yearParam)
                    .ToListAsync();

                var totalPaid = result
                    .Where(x => x.Status == "Paid")
                    .Sum(x => x.Amount);

                var remainingAmount = fee.TotalFee - totalPaid;
                var totalLateFine = result.Sum(x => x.Fine);
                var paidMonths = result.Count(x => x.Status == "Paid" && x.Amount > 0);

                var payments = await _context.FeePayments
                     .AsNoTracking()
                .Where(x => x.FeeId == fee.FeeId) // optional filter
                .OrderByDescending(x => x.PaymentDate) // latest first
                .ToListAsync();

                // 8. Return ViewModel
                return new StudentFeeDetailViewModel
                {
                    Student = student,
                    Fee = fee,
                    Installments = result,
                    PaymentHis = payments,

                    TotalFee = fee.TotalFee,
                    Discount = fee.DiscountApplied ?? 0,
                    TotalPaid = totalPaid,
                    RemainingAmount = remainingAmount,
                    LateFine = totalLateFine,

                    PaidMonths = paidMonths,

                    IsInstallment = fee.PaymentOption == "Installment"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while reading student fees detail!!");
                return new StudentFeeDetailViewModel();
            }
        }

        public async Task<FeePaymentViewModel> ReadFeePaymentAsync(int userId, int month)
        {
            decimal lateFine = 0;

            var studentId = _context.Students
                .Where(s => s.UserID == userId)
                .Select(s => s.StudentID)
                .FirstOrDefault();

            // 1. Get latest fee (better: filter by AcademicYear if needed)
            var fee = await _context.StudentFees
                .Where(stu => stu.StudentId == studentId)
                .OrderByDescending(stu => stu.FeeId)
                .FirstOrDefaultAsync();

            if (fee == null)
                return null;

            // Current installment date
            int dueDay = await _context.FeeConfigurations
                .Where(fc => fc.IsActive)
                .Select(fc => fc.DueDay).FirstOrDefaultAsync();

            var dueDate = new DateTime(DateTime.Now.Year, month, dueDay);

            // Check fee waived
            //var isFeeWaived = await IsMonthExempted(dueDate);
            //var waivedReason = await GetMonthExemptionReason(dueDate); 
            var exemption = await GetMonthExemption(dueDate);
            var isFeeWaived = exemption.IsExempted;
            var waivedReason = exemption.Reason;

            bool alreadyPaid = _context.FeePayments
                        .Any(x => x.FeeMonth == month && fee.FeeId == x.FeeId);

            if (alreadyPaid)
            {
                throw new Exception("Sorry not allowed to pay.");
            }

            var totalPaid = _context.FeePayments
                .Where(x => x.FeeId == fee.FeeId)
                .Sum(x => (decimal?)(x.AmountPaid - x.LateFine)) ?? 0;

            // No late fine if waived
            if (!isFeeWaived && DateTime.Today > dueDate)
            {
                var fineAmt = await _context.FeeConfigurations
                .Where(fc => fc.IsActive)
                .Select(fc => fc.LateFineAmount).FirstOrDefaultAsync();

                int lateMonths = ((DateTime.Today.Year - dueDate.Year) * 12)
                    + DateTime.Today.Month - dueDate.Month;

                lateFine = lateMonths * fineAmt;
            }
            var finalAmount = isFeeWaived ? 0 : fee.MonthlyAmount + lateFine;

            //// Previous Payments
            //var previousPayments = await _context.FeePayments
            //    .Where(x => x.FeeId == fee.FeeId)
            //    .OrderBy(x => x.InstallmentNo)
            //    .ToListAsync();

            // 2. Student Info
            var student = await (
                from s in _context.Students
                join se in _context.StudentEnrollments on s.StudentID equals se.StudentId
                //join cs in _context.ClassSections on s.ClassSectionId equals cs.Id
                join c in _context.Classes on se.ClassId equals c.ClassId
                join sec in _context.Sections on se.SectionId equals sec.SectionId
                join ay in _context.AcademicYears on s.AcademicYearID equals ay.AcademicYearID
                where s.StudentID == studentId && se.Status == "Active"
                select new StudentInfoViewModel
                {
                    StudentId = s.StudentID,
                    StudentName = s.StudentName,
                    Class = c.ClassName,
                    Section = sec.SectionName,
                    AdmissionNumber = s.AdmissionNumber
                }).FirstOrDefaultAsync();


            var vm = new FeePaymentViewModel
            {
                FeeId = fee.FeeId,
                StudentInfoView = student,
                TotalFee = fee.TotalFee,
                PaidAmount = totalPaid,
                PendingAmount = fee.TotalFee - totalPaid,
                InstallmentNo = fee.PaidMonths.Value + 1,
                MonthlyAmount = fee.MonthlyAmount,
                DueDate = dueDate,
                LateFine = lateFine,
                FinalAmount = finalAmount,
                IsFeeWaived = isFeeWaived,
                //PreviousPayments = previousPayments,
                WaivedReason = waivedReason
            };

            return vm;
        }

        public async Task<FeePaymentViewModel> CalculateFeesAmount(int userId, int month)
        {
            decimal lateFine = 0;

            var studentId = _context.Students
                .Where(s => s.UserID == userId)
                .Select(s => s.StudentID)
                .FirstOrDefault();

            // 1. Get latest fee (better: filter by AcademicYear if needed)
            var fee = await _context.StudentFees
                .Where(stu => stu.StudentId == studentId)
                .OrderByDescending(stu => stu.FeeId)
                .FirstOrDefaultAsync();

            if (fee == null)
                return null;

            // Current installment date
            int dueDay = await _context.FeeConfigurations
                .Where(fc => fc.IsActive)
                .Select(fc => fc.DueDay).FirstOrDefaultAsync();

            var dueDate = new DateTime(DateTime.Now.Year, month, dueDay);

            var exemption = await GetMonthExemption(dueDate);
            var isFeeWaived = exemption.IsExempted;
            var waivedReason = exemption.Reason;

            //var totalPaid = _context.FeePayments
            //    .Where(x => x.FeeId == fee.FeeId)
            //    .Sum(x => (decimal?)(x.AmountPaid - x.LateFine)) ?? 0;

            // No late fine if waived
            if (!isFeeWaived && DateTime.Today > dueDate)
            {
                var fineAmt = await _context.FeeConfigurations
                .Where(fc => fc.IsActive)
                .Select(fc => fc.LateFineAmount).FirstOrDefaultAsync();

                int lateMonths = ((DateTime.Today.Year - dueDate.Year) * 12)
                    + DateTime.Today.Month - dueDate.Month;

                lateFine = lateMonths * fineAmt;
            }
            var finalAmount = isFeeWaived ? 0 : fee.MonthlyAmount + lateFine;

            var vm = new FeePaymentViewModel
            {
                FeeId = fee.FeeId,
                //TotalFee = fee.TotalFee,
                //PaidAmount = totalPaid,
                //PendingAmount = fee.TotalFee - totalPaid,
                InstallmentNo = fee.PaidMonths.Value + 1,
                MonthlyAmount = fee.MonthlyAmount,
                DueDate = dueDate,
                LateFine = lateFine,
                FinalAmount = finalAmount,
                //IsFeeWaived = isFeeWaived,
                //WaivedReason = waivedReason
            };

            return vm;
        }


        /* SELECT* FROM StudentFees
            if any installment payment was done then it is inserted in FeePayments then after that 
            UPDATE StudentFees SET PaidMonths = (PaidMonths + 1) where FeeId = 1
            SELECT* FROM FeePayments */

        public async Task<bool> ProcessPaymentAsync(FeePaymentViewModel model)
        {
            var paymentMethod = model.SelectedPaymentMethod;

            // Example

            if (model.SelectedPaymentMethod == PaymentMethod.Cash)
            {
                // Generate receipt
            }

            if (model.SelectedPaymentMethod == PaymentMethod.UPI)
            {
                // UPI flow
            }

            if (model.SelectedPaymentMethod == PaymentMethod.Card)
            {
                // Card flow
            }

            var fee = await _context.StudentFees.FirstOrDefaultAsync(x => x.FeeId == model.FeeId);

            if (fee == null)
                return false;

            // Recalculate security-side
            var finalAmount = model.MonthlyAmount + model.LateFine;

            var payment = new FeePayment
            {
                FeeId = model.FeeId,
                InstallmentNo = model.InstallmentNo,
                DueDate = model.DueDate,
                Amount = model.MonthlyAmount,
                LateFine = model.LateFine,
                AmountPaid = finalAmount,
                PaymentDate = DateTime.Now,
                PaymentMethod = model.SelectedPaymentMethod.ToString(),
                TransactionID = Guid.NewGuid().ToString(),
                FeeMonth = DateTime.Now.Month,
                FeeYear = DateTime.Now.Year
            };

            _context.FeePayments.Add(payment);

            // Update StudentFees
            fee.PaidMonths += 1;
            //fee.RemainingAmount -= finalAmount;
            fee.RemainingAmount -= model.MonthlyAmount;

            if (fee.RemainingAmount <= 0)
            {
                fee.Status = "PAID";
            }

            await _context.SaveChangesAsync();

            return true;
        }


    }
}
