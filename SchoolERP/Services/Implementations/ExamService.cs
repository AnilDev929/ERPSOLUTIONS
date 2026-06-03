using Microsoft.EntityFrameworkCore;
using SchoolERP.Common;
using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;

namespace SchoolERP.Services.Implementations
{
    public class ExamService : IExamService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ExamService> _logger;

        public ExamService(AppDbContext context, ILogger<ExamService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ExamVM>> GetAllAsync()
        {
            return await _context.Exams
                .Include(x => x.AcademicYear)
                .Select(x => new ExamVM
                {
                    ExamId = x.ExamId,
                    ExamName = x.ExamName,
                    Description = x.Description,
                    AcademicYearID = x.AcademicYearID,
                    AcademicYearName = x.AcademicYear.YearName
                })
                .ToListAsync();

        }

        public async Task<Exam?> GetByIdAsync(int id)
        {
            return await _context.Exams.FindAsync(id);
        }

        public async Task<(bool Success, string Message)> CreateAsync(Exam exam)
        {
            try
            {
                var exists = await _context.Exams
                    .AnyAsync(x => x.ExamName == exam.ExamName
                                && x.AcademicYearID == exam.AcademicYearID);

                if (exists)
                    return (false, "Exam already exists for selected academic year.");

                _context.Exams.Add(exam);
                await _context.SaveChangesAsync();

                return (true, "Exam created successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateAsync(Exam exam)
        {
            try
            {
                var duplicate = await _context.Exams
                    .AnyAsync(x => x.ExamName == exam.ExamName
                                && x.AcademicYearID == exam.AcademicYearID
                                && x.ExamId != exam.ExamId);

                if (duplicate)
                    return (false, "Another exam with same name already exists.");

                _context.Exams.Update(exam);
                await _context.SaveChangesAsync();

                return (true, "Exam updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }
























        public async Task<ServiceResponse> CreateExamAsync(Exam exam)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(exam.ExamName))
                    return ServiceResponse.Fail("Exam name is required");

                await _context.Exams.AddAsync(exam);
                await _context.SaveChangesAsync();

                return ServiceResponse.Ok("Exam created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exam");
                return ServiceResponse.Fail("Failed to create exam");
            }
        }

        public async Task<ServiceResponse> SaveScheduleAsync(List<ExamSchedule> schedules)
        {
            using var trx = await _context.Database.BeginTransactionAsync();

            try
            {
                if (!schedules.Any())
                    return ServiceResponse.Fail("No schedule data");

                await _context.ExamSchedules.AddRangeAsync(schedules);
                await _context.SaveChangesAsync();

                await trx.CommitAsync();
                return ServiceResponse.Ok("Schedule saved");
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                _logger.LogError(ex, "Schedule error");
                return ServiceResponse.Fail("Error saving schedule");
            }
        }

        public async Task<ServiceResponse> SaveGradeRulesAsync(List<GradeRule> rules)
        {
            try
            {
                await _context.GradeRules.AddRangeAsync(rules);
                await _context.SaveChangesAsync();

                return ServiceResponse.Ok("Grades saved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Grade error");
                return ServiceResponse.Fail("Error saving grades");
            }
        }
    }
}
