using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SchoolERP.Services.Implementations
{
    public class AcademicYearService : IAcademicYearService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AcademicYearService> _logger;

        public AcademicYearService(AppDbContext context, ILogger<AcademicYearService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IEnumerable<AcademicYearDropdownDto> GetAll()
        {
            return _context.AcademicYears
                           .Where(a => a.IsActive)
                           .OrderByDescending(a => a.YearStart)
                           .Select(a => new AcademicYearDropdownDto
                           {
                               AcademicYearID = a.AcademicYearID,
                               YearName = a.YearName
                           })
                           .ToList();
        }

        public IEnumerable<AcademicYear> GetAllAsync()
        {
            return _context.AcademicYears
                .OrderByDescending(x => x.YearStart)
                .ToList();
        }

        public async Task<AcademicYear?> GetByIdAsync(int id)
        {
            return await _context.AcademicYears.FindAsync(id);
        }

        public async Task<string> AddAsync(AcademicYear model)
        {
            try
            {
                // ✅ 1. Check duplicate year
                var exists = await _context.AcademicYears
                    .AnyAsync(x => x.YearName == model.YearName);

                if (exists)
                    return "duplicate";

                // ✅ 2. Ensure only one active year
                if (model.IsActive)
                {
                    var activeYears = _context.AcademicYears.Where(x => x.IsActive);

                    foreach (var item in activeYears)
                        item.IsActive = false;
                }

                // ✅ 3. Add new record
                _context.AcademicYears.Add(model);
                await _context.SaveChangesAsync();

                return "Success";
            }
            catch(Exception ex) {
                _logger.LogError(ex, "Error in AddAsync() !!");
                return "Error";
            }
        }

        public async Task<string> UpdateAsync(AcademicYear model)
        {
            // ✅ 1. Check duplicate Academic Year (exclude current record)
            var isDuplicate = await _context.AcademicYears
                .AnyAsync(x => x.YearName == model.YearName
                            && x.AcademicYearID != model.AcademicYearID);
            if (isDuplicate)
                return "exists";

            // ✅ 2. Ensure only one active Academic Year
            if (model.IsActive)
            {
                var activeYears = await _context.AcademicYears
                    .Where(x => x.IsActive && x.AcademicYearID != model.AcademicYearID)
                    .ToListAsync();

                foreach (var item in activeYears)
                {
                    item.IsActive = false;
                }
            }

            // ✅ 3. Update current record safely
            var existing = await _context.AcademicYears
                .FirstOrDefaultAsync(x => x.AcademicYearID == model.AcademicYearID);

            if (existing == null)
                return "notfound.";

            existing.YearName = model.YearName;
            existing.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            return "updated.";
        }

        private async Task<bool> IsDuplicateAsync(string yearName, int id = 0)
        {
            return await _context.AcademicYears
                .AnyAsync(x => x.YearName == yearName && x.AcademicYearID != id);
        }

        public async Task DeleteAsync(int id)
        {
            var data = await _context.AcademicYears.FindAsync(id);
            if (data != null)
            {
                data.IsActive = false;
                //_context.AcademicYears.Remove(data);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<AcademicYear> GetCurrentAcademicYearAsync()
        {
            var year = await _context.AcademicYears
                .FirstOrDefaultAsync(x => x.IsActive);

            if (year == null)
                throw new Exception("No active academic year found.");

            return year;
        }
    }
}
