using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SchoolERP.Services.Implementations
{
    public class SubjectService : ISubjectService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SubjectService> _logger;

        public SubjectService(AppDbContext context, ILogger<SubjectService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Create
        public async Task<bool> AddSubjectAsync(Subject subject)
        {
            try
            {
                // Duplicate check
                if (await _context.Subjects
                    .AnyAsync(s => s.SubjectName == subject.SubjectName ||
                                   (subject.SubjectCode != null && s.SubjectCode == subject.SubjectCode)))
                {
                    _logger.LogWarning("Duplicate subject detected: {SubjectName} / {SubjectCode}",
                                        subject.SubjectName, subject.SubjectCode);
                    return false;
                }

                await _context.Subjects.AddAsync(subject);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Subject '{SubjectName}' added successfully.", subject.SubjectName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddSubjectAsync failed for {SubjectName}", subject.SubjectName);
                return false;
            }
        }

        // Read All
        public async Task<List<Subject>> GetAllSubjectsAsync()
        {
            try
            {
                return await _context.Subjects.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllSubjectsAsync failed.");
                return new List<Subject>();
            }
        }

        // Read By Id
        public async Task<Subject> GetSubjectByIdAsync(int id)
        {
            try
            {
                return await _context.Subjects.AsNoTracking()
                                              .FirstOrDefaultAsync(s => s.SubjectId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSubjectByIdAsync failed for Id {Id}", id);
                return null;
            }
        }

        // Update
        public async Task<bool> UpdateSubjectAsync(Subject subject)
        {
            try
            {
                if (await _context.Subjects
                    .AnyAsync(s => s.SubjectId != subject.SubjectId &&
                                   (s.SubjectName == subject.SubjectName ||
                                    (subject.SubjectCode != null && s.SubjectCode == subject.SubjectCode))))
                {
                    _logger.LogWarning("Duplicate subject detected on update: {SubjectName} / {SubjectCode}",
                                        subject.SubjectName, subject.SubjectCode);
                    return false;
                }

                _context.Subjects.Update(subject);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Subject '{SubjectName}' updated successfully.", subject.SubjectName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateSubjectAsync failed for {SubjectName}", subject.SubjectName);
                return false;
            }
        }

        // Delete
        public async Task<bool> DeleteSubjectAsync(int id)
        {
            try
            {
                var subject = await _context.Subjects.FindAsync(id);
                if (subject == null)
                {
                    _logger.LogWarning("Subject with ID {Id} not found.", id);
                    return false;
                }

                _context.Subjects.Remove(subject);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Subject '{SubjectName}' deleted successfully.", subject.SubjectName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteSubjectAsync failed for Id {Id}", id);
                return false;
            }
        }
    }

}
