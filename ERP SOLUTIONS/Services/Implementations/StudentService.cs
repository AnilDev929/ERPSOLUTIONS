using ERP_SOLUTIONS.Data;
using ERP_SOLUTIONS.Models.Entities;
using ERP_SOLUTIONS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_SOLUTIONS.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;

        public StudentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Student>> GetAllAsync()
            => await _context.Students.ToListAsync();

        public async Task<Student> GetByIdAsync(int id)
            => await _context.Students.FindAsync(id);

        public async Task AddAsync(Student student)
        {
            // Business Rule Example
            if (string.IsNullOrEmpty(student.StudentName))
                throw new Exception("Student name is required");

            _context.Students.Add(student);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var student = await GetByIdAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
        }
    }
}
