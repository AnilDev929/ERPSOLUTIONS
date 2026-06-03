using Microsoft.EntityFrameworkCore;
using SchoolERP.Common;
using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;

namespace SchoolERP.Services.Implementations
{
    public class HolidayService : IHolidayService
    {
        private readonly AppDbContext _context;

        public HolidayService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Holiday>> GetAllAsync()
        {
            return await _context.Holidays
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync();
        }

        public async Task<Holiday> GetByIdAsync(int id)
        {
            return await _context.Holidays.FindAsync(id);
        }

        public async Task<ServiceResponse> CreateAsync(Holiday model)
        {
            try
            {
                bool exists = await _context.Holidays.AnyAsync(x =>
                    x.Title == model.Title &&
                    x.StartDate == model.StartDate &&
                    x.EndDate == model.EndDate &&
                    x.IsActive);

                if (exists)
                    return ServiceResponse.Fail("Holiday already exists.");

                _context.Holidays.Add(model);
                await _context.SaveChangesAsync();

                return ServiceResponse.Ok("Holiday created successfully.");
            }
            catch (Exception ex)
            {
               return ServiceResponse.Fail(ex.Message);
            }
        }

        public async Task<ServiceResponse> UpdateAsync(Holiday model)
        {
            try
            {
                var existing = await _context.Holidays.FindAsync(model.Id);
                if (existing == null)
                    return ServiceResponse.Fail("Holiday not found.");

                existing.Title = model.Title;
                existing.StartDate = model.StartDate;
                existing.EndDate = model.EndDate;
                existing.Type = model.Type;
                existing.Description = model.Description;
                existing.IsRecurring = model.IsRecurring;

                await _context.SaveChangesAsync();

                return ServiceResponse.Ok("Updated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse.Fail(ex.Message);
            }
        }

        public async Task<ServiceResponse> DeleteAsync(int id)
        {
            var data = await _context.Holidays.FindAsync(id);
            if (data == null) return ServiceResponse.Fail("Failed to delete record.");

            data.IsActive = false;
            await _context.SaveChangesAsync();
            return ServiceResponse.Fail("Record deleted.");
        }
    }
}
