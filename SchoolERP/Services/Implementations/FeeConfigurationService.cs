using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SchoolERP.Services.Implementations
{
    public class FeeConfigurationService : IFeeConfigurationService
    {
        private readonly AppDbContext _context;

        public FeeConfigurationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<FeeConfiguration>> GetAllAsync()
        {
            return await _context.FeeConfigurations.
                OrderByDescending(f => f.AcademicYearID)
                .ToListAsync();
        }

        public async Task<FeeConfiguration> GetByIdAsync(int id)
        {
            return await _context.FeeConfigurations.FindAsync(id);
        }

        public async Task CreateAsync(FeeConfiguration entity)
        {
            if (entity.IsActive)
            {
                var existingActive = await _context.FeeConfigurations
                    .Where(x => x.IsActive)
                    .ToListAsync();

                foreach (var item in existingActive)
                {
                    item.IsActive = false;
                }
            }

            _context.FeeConfigurations.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FeeConfiguration entity)
        {
            var existing = await _context.FeeConfigurations
                .FirstOrDefaultAsync(x => x.ConfigId == entity.ConfigId);

            if (existing == null)
                return;

            if (entity.IsActive)
            {
                var others = await _context.FeeConfigurations
                    .Where(x => x.IsActive && x.ConfigId != entity.ConfigId)
                    .ToListAsync();

                foreach (var item in others)
                {
                    item.IsActive = false;
                }
            }

            existing.AcademicYearID = entity.AcademicYearID;
            existing.FeeType = entity.FeeType;
            existing.DueDay = entity.DueDay;
            existing.GraceDays = entity.GraceDays;
            existing.LateFineType = entity.LateFineType;
            existing.LateFineAmount = entity.LateFineAmount;
            existing.MaxMonthsPerPayment = entity.MaxMonthsPerPayment;
            existing.IsActive = entity.IsActive;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var data = await GetByIdAsync(id);
            if (data != null)
            {
                data.IsActive = false;
                //_context.FeeConfigurations.Remove(data);
                await _context.SaveChangesAsync();
            }
        }
    }
}
