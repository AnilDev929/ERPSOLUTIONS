using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SchoolERP.Services.Implementations
{
    public class CourseFeeService : ICourseFeeService
    {
        private readonly AppDbContext _context;

        public CourseFeeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<object>> GetAllAsync(int? classId, int? yearId)
        {
            var query = _context.CourseFees
                .Where(x => !x.IsDeleted);

            // ✅ Apply filters only if values are provided
            if (classId.HasValue && classId.Value > 0)
            {
                query = query.Where(x => x.ClassId == classId.Value);
            }

            if (yearId.HasValue && yearId.Value > 0)
            {
                query = query.Where(x => x.AcademicYearId == yearId.Value);
            }

            // ✅ Projection (after filtering)
            return await query
                .Select(x => new
                {
                    x.CourseFeeId,
                    x.ClassId,
                    ClassName = x.Class.ClassName,
                    x.AcademicYearId,
                    Year = x.AcademicYear.YearName,
                    x.AdmissionFee,
                    x.TuitionFee,
                    x.TransportFee,
                    x.LibraryFee,
                    x.OtherFee,
                    x.TotalFee,
                    x.IsActive
                })
                .ToListAsync<object>();
        }

        public async Task<CourseFee> GetByIdAsync(int id)
        {
            var data = await _context.CourseFees.FindAsync(id);

            if (data == null)
                throw new Exception("Record not found");

            return data;
        }

        public async Task<(bool success, string message)> SaveAsync(CourseFee model)
        {
            using var t = await _context.Database.BeginTransactionAsync();

            try
            {
                // ✅ Duplicate check
                bool exists = await _context.CourseFees.AnyAsync(x =>
                    x.ClassId == model.ClassId &&
                    x.AcademicYearId == model.AcademicYearId &&
                    x.CourseFeeId != model.CourseFeeId &&
                    !x.IsDeleted);

                if (exists)
                    throw new InvalidOperationException("Fee configuration already exists for this class and academic year.");

                // ✅ Only ONE active fee per Class + AcademicYear
                //NOT global system - wide deactivation
                if (model.IsActive)
                {
                    var query = _context.CourseFees.Where(x =>
                        x.IsActive &&
                        !x.IsDeleted &&
                        x.ClassId == model.ClassId &&
                        x.AcademicYearId == model.AcademicYearId &&
                        x.CourseFeeId != model.CourseFeeId);

                    await query.ForEachAsync(x => x.IsActive = false);
                }

                if (model.CourseFeeId == 0)
                    await _context.CourseFees.AddAsync(model);
                else
                {
                    var existing = await _context.CourseFees
                        .FirstOrDefaultAsync(x => x.CourseFeeId == model.CourseFeeId);

                    if (existing == null)
                        return (false, "Record not found");

                    // update fields manually (best practice)
                    existing.ClassId = model.ClassId;
                    existing.AcademicYearId = model.AcademicYearId;
                    existing.TuitionFee = model.TuitionFee;
                    existing.AdmissionFee = model.AdmissionFee;
                    existing.TransportFee = model.TransportFee;
                    existing.LibraryFee = model.LibraryFee;
                    existing.OtherFee = model.OtherFee;
                    existing.IsActive = model.IsActive;
                }

                await _context.SaveChangesAsync();
                await t.CommitAsync();

                return (true, "Saved successfully!");
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                return (false, ex.Message);
            }
        }

        public async Task<(bool success, string message)> DeleteAsync(int id)
        {
            try
            {
                var entity = await _context.CourseFees.FindAsync(id);

                if (entity == null)
                    return (false, "Record not found");

                entity.IsDeleted = true;

                await _context.SaveChangesAsync();

                return (true, "Deleted successfully!");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<object> GetDropdownsAsync()
        {
            var classes = await _context.Classes
                .Select(x => new { x.ClassId, x.ClassName })
                .ToListAsync();

            var years = await _context.AcademicYears
                .Select(x => new { x.AcademicYearID, x.YearName })
                .ToListAsync();

            return new { classes, years };
        }
    }

}
