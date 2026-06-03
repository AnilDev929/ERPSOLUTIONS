using Microsoft.EntityFrameworkCore;
using SchoolERP.Common;
using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;

namespace SchoolERP.Services.Implementations
{
    public class StudentPromotionService
    {
        private readonly AppDbContext _context;

        public StudentPromotionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse> PromoteStudentsAsync(PromotionViewModel model, int userId)
        {
            var cls = await _context.Classes.FindAsync(model.FromClassId);
            string className = cls.ClassName;

            // 1. Check terminal class (10th rule)
            if (className.Contains("10"))
            {
                return ServiceResponse.Fail("Students of 10th class cannot be promoted. Mark them as Passed Out.");
            }
            // 2. Validate students
            if (model.StudentIds == null || !model.StudentIds.Any())
            {
                return ServiceResponse.Fail("No students selected.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var studentId in model.StudentIds)
                {
                    // 1. Get active enrollment
                    var oldEnrollment = await _context.StudentEnrollments
                        .FirstOrDefaultAsync(x => x.StudentId == studentId && x.IsActive);

                    if (oldEnrollment == null)
                        continue;

                    // 2. Deactivate old record
                    oldEnrollment.IsActive = false;
                    oldEnrollment.Status = "Promoted";

                    // 3. Generate roll number (safe)
                    int rollNo = await GenerateRollNumberAsync(
                        model.ToClassId,
                        model.ToSectionId,
                        model.ToAcademicYearId);

                    // 4. Create new enrollment
                    var newEnrollment = new StudentEnrollment
                    {
                        StudentId = studentId,
                        ClassId = model.ToClassId,
                        SectionId = model.ToSectionId,
                        AcademicYearId = model.ToAcademicYearId,
                        RollNumber = rollNo,
                        Status = "Active",
                        IsActive = true,
                        IsPromoted = true,
                        IsNewAdmission = false,
                        CreatedDate = DateTime.Now
                    };

                    /*
                     * 
                     if (enrollment.IsNewAdmission)
{
    totalFee = fee.AdmissionFee + fee.TuitionFee;
}
else if (enrollment.IsPromoted)
{
    totalFee = fee.TuitionFee; // ❌ No admission fee
}
                     */


                    _context.StudentEnrollments.Add(newEnrollment);
                    await _context.SaveChangesAsync();

                    // 5. Log history (Insert promotion log)
                    var log = new StudentPromotionLog
                    {
                        StudentId = studentId,

                        FromClassId = model.FromClassId,
                        ToClassId = model.ToClassId,

                        FromSectionId = model.FromSectionId,
                        ToSectionId = model.ToSectionId,

                        FromAcademicYearId = model.FromAcademicYearId,
                        ToAcademicYearId = model.ToAcademicYearId,

                        OldEnrollmentId = oldEnrollment.EnrollmentId,
                        NewEnrollmentId = newEnrollment.EnrollmentId,

                        RollNumber = rollNo,
                        PromotionDate = DateTime.Now,
                        PromotedBy = userId
                    };

                    _context.StudentPromotionLogs.Add(log);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResponse.Ok("Students promoted successfully");
                //return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return ServiceResponse.Fail("Unable to promote Students. Some Error occured");
                //throw;
            }
        }

        private async Task<int> GenerateRollNumberAsync(int classId, int sectionId, int yearId)
        {
            var maxRoll = await _context.StudentEnrollments
                .Where(x => x.ClassId == classId
                         && x.SectionId == sectionId
                         && x.AcademicYearId == yearId)
                .MaxAsync(x => (int?)x.RollNumber);

            return (maxRoll ?? 0) + 1;
        }

        private async Task ValidatePromotionWindow(int academicYearId)
        {
            var setting = await _context.PromotionSettings
                .FirstOrDefaultAsync(x => x.AcademicYearId == academicYearId);

            if (setting == null || !setting.IsPromotionOpen)
                throw new Exception("Promotion window is closed.");

            if (setting.IsLocked)
                throw new Exception("Promotion already finalized.");

            if (DateTime.UtcNow < setting.PromotionStartDate ||
                DateTime.UtcNow > setting.PromotionEndDate)
                throw new Exception("Outside promotion window.");
        }

        //public async Task<PromotionState> GetPromotionStateAsync(int academicYearId)
        //{
        //    var setting = await _context.PromotionSettings
        //        .FirstOrDefaultAsync(x => x.AcademicYearId == academicYearId);

        //    if (setting == null)
        //        return new PromotionState();

        //    var now = DateTime.UtcNow;

        //    return new PromotionState
        //    {
        //        IsOpen = setting.IsPromotionOpen,
        //        IsLocked = setting.IsLocked,
        //        IsWithinDateRange = now >= setting.PromotionStartDate &&
        //                            now <= setting.PromotionEndDate
        //    };
        //}


    }
}
