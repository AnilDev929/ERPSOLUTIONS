using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SchoolERP.Services.Implementations
{
    public class ClassService : IClassService
    {

        private readonly AppDbContext _context;
        private readonly ILogger<ClassService> _logger;

        public ClassService(AppDbContext context, ILogger<ClassService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ClassModel>> GetAllClassesAsync()
        {
            return await _context.Classes.ToListAsync();
        }

        public async Task<ClassModel> GetClassByIdAsync(int id)
        {
            return await _context.Classes.FindAsync(id);
        }

        public async Task<bool> AddClassAsync(ClassModel newClass)
        {
            try
            {
                // Check for duplicate name (case-insensitive)
                bool exists = await _context.Classes
                    .AnyAsync(c => c.ClassName.ToLower() == newClass.ClassName.ToLower());
                if (exists) return false;

                await _context.Classes.AddAsync(newClass);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Added new class: {newClass.ClassName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding class");
                return false;
            }
        }

        public async Task<bool> UpdateClassAsync(ClassModel updatedClass)
        {
            try
            {
                // Check for duplicate name excluding current
                bool exists = await _context.Classes
                    .AnyAsync(c => c.ClassName.ToLower() == updatedClass.ClassName.ToLower() && c.ClassId != updatedClass.ClassId);
                if (exists) return false;

                _context.Classes.Update(updatedClass);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Updated class: {updatedClass.ClassName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating class");
                return false;
            }
        }

        public async Task<bool> DeleteClassAsync(int id)
        {
            try
            {
                var cls = await _context.Classes.FindAsync(id);
                if (cls == null) return false;

                _context.Classes.Remove(cls);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Deleted class with ID: {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting class");
                return false;
            }
        }
    }
}
