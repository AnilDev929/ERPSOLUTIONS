using Microsoft.EntityFrameworkCore;
using SchoolERP.Common;
using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;

namespace SchoolERP.Services.Implementations
{
    public class PromotionService : IPromotionService
    {
        private readonly AppDbContext _context;

        public PromotionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PromotionState> GetPromotionStateAsync(int academicYearId)
        {
            var setting = await _context.PromotionSettings
                .FirstOrDefaultAsync(x => x.AcademicYearId == academicYearId);

            if (setting == null)
            {
                return new PromotionState
                {
                    IsOpen = false,
                    IsLocked = true,
                    IsWithinDateRange = false
                };
            }

            var now = DateTime.UtcNow;

            return new PromotionState
            {
                IsOpen = setting.IsPromotionOpen,
                IsLocked = setting.IsLocked,
                IsWithinDateRange =
                    now >= setting.PromotionStartDate &&
                    now <= setting.PromotionEndDate
            };
        }

        public async Task<PromotionSetting> GetByAcademicYearAsync(int academicYearId)
        {
            return await _context.PromotionSettings
                .FirstOrDefaultAsync(x => x.AcademicYearId == academicYearId);
        }

        // 🔥 MAIN SAVE METHOD
        public async Task<ServiceResponse> SaveAsync(PromotionSetting model)
        {
            var validation = await ValidateAsync(model);
            if (!validation.Success)
                return validation;

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var existing = await _context.PromotionSettings
                    .FirstOrDefaultAsync(x => x.AcademicYearId == model.AcademicYearId);

                if (existing == null)
                {
                    _context.PromotionSettings.Add(model);
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    return ServiceResponse.Ok("Promotion settings created successfully.");
                }
                else
                {
                    existing.PromotionStartDate = model.PromotionStartDate;
                    existing.PromotionEndDate = model.PromotionEndDate;
                    existing.IsPromotionOpen = model.IsPromotionOpen;
                    existing.IsLocked = model.IsLocked;

                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    return ServiceResponse.Ok("Promotion settings updated successfully.");
                }
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync();
                return ServiceResponse.Fail("Duplicate record detected. Only one setting per academic year is allowed.");
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                return ServiceResponse.Fail("Unexpected error occurred while saving promotion settings.");
            }
        }

        // 🔐 VALIDATION + DUPLICATE CHECK
        public async Task<ServiceResponse> ValidateAsync(PromotionSetting model)
        {
            if (model.PromotionStartDate > model.PromotionEndDate)
                return ServiceResponse.Fail("Start date cannot be greater than end date.");

            // ✅ Allow update: ignore same record
            var existing = await _context.PromotionSettings
                .FirstOrDefaultAsync(x =>
                    x.AcademicYearId == model.AcademicYearId &&
                    x.Id != model.Id); // 👈 KEY FIX

            if (existing != null)
                return ServiceResponse.Fail("Promotion settings already exist for this academic year.");

            //// 🔥 Duplicate check: Only ONE active promotion window per year
            //var existing = await _context.PromotionSettings
            //    .FirstOrDefaultAsync(x => x.AcademicYearId == model.AcademicYearId);

            //// 🔴 KEY FIX
            //if (existing != null)
            //    return ServiceResponse.Fail("Promotion settings already exist for this academic year.");

            // 🔥 Prevent invalid lock state
            if (model.IsLocked && model.IsPromotionOpen)
                return ServiceResponse.Fail("Cannot keep promotion open when it is locked.");

            return ServiceResponse.Ok("No Duplicate");
        }

        public async Task<List<PromotionSettingDto>> GetPromotionSettingsAsync()
        {
            var data = await _context.PromotionSettings
                .Include(x => x.AcademicYear) 
                    .Select(x => new PromotionSettingDto
                    {
                        Id = x.Id,
                        AcademicYear = x.AcademicYear.YearName,
                        StartDate = x.PromotionStartDate.ToString("yyyy-MM-dd"),
                        EndDate = x.PromotionEndDate.ToString("yyyy-MM-dd"),
                        IsOpen = x.IsPromotionOpen,
                        IsLocked = x.IsLocked
                    })
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();

            return data;
        }

        public async Task<PromotionSetting?> GetByIdAsync(int id)
        {
            return await _context.PromotionSettings.FindAsync(id);
        }
    }
}
