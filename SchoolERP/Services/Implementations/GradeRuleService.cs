using Microsoft.EntityFrameworkCore;
using SchoolERP.Common;
using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;

namespace SchoolERP.Services.Implementations
{
    public class GradeRuleService : IGradeRuleService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GradeRuleService> _logger;

        public GradeRuleService(AppDbContext context, ILogger<GradeRuleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<GradeRule>> GetAllAsync()
        {
            return await _context.GradeRules
                .OrderByDescending(x => x.MaxScore)
                .ToListAsync();
        }
        public async Task<GradeRule?> GetByIdAsync(int id)
        {
            return await _context.GradeRules.FindAsync(id);
        }

        public async Task ValidateAsync(GradeRuleDto dto)
        {
            if (dto.MinScore < 0 || dto.MaxScore > 100)
                throw new Exception("Score must be between 0 and 100.");

            if (dto.MinScore > dto.MaxScore)
                throw new Exception("MinScore cannot be greater than MaxScore.");

            var overlap = await _context.GradeRules.AnyAsync(x =>
                x.GradeRuleID != dto.GradeRuleID &&
                (
                    (dto.MinScore >= x.MinScore && dto.MinScore <= x.MaxScore) ||
                    (dto.MaxScore >= x.MinScore && dto.MaxScore <= x.MaxScore)
                )
            );

            if (overlap)
                throw new Exception("Score range overlaps with existing rule.");
        }

        public async Task<bool> IsDuplicateAsync(GradeRuleDto dto)
        {
            return await _context.GradeRules.AnyAsync(x =>
                x.RuleName == dto.RuleName &&
                x.GradeLetter == dto.GradeLetter &&
                x.GradeRuleID != dto.GradeRuleID
            );
        }

        //public async Task<int> CreateOrUpdateAsync(GradeRuleDto dto)
        //{
        //    await ValidateAsync(dto);

        //    if (await IsDuplicateAsync(dto))
        //        throw new Exception("Duplicate grade rule found.");

        //    var entity = _mapper.Map<GradeRule>(dto);
        //    _db.GradeRule.Add(entity);
        //    await _db.SaveChangesAsync();

        //    return entity.GradeRuleID;
        //}

        //public async Task<bool> DuplicateAsync(int id)
        //{
        //    var original = await _db.GradeRule.FindAsync(id);
        //    if (original == null) return false;

        //    var copy = new GradeRule
        //    {
        //        RuleName = original.RuleName + " Copy",
        //        MinScore = original.MinScore,
        //        MaxScore = original.MaxScore,
        //        GradeLetter = original.GradeLetter,
        //        GradePoint = original.GradePoint,
        //        ResultStatus = original.ResultStatus,
        //        DisplayOrder = original.DisplayOrder + 1,
        //        ColorCode = original.ColorCode,
        //        IsActive = true
        //    };

        //    _db.GradeRule.Add(copy);
        //    await _db.SaveChangesAsync();

        //    return true;
        //}

        public async Task<ServiceResponse> CreateAsync(GradeRule rule)
        {
            try
            {
                if (rule.MinScore > rule.MaxScore)
                    return ServiceResponse.Fail("Min percentage cannot be greater than Max percentage.");

                var overlap = await _context.GradeRules.AnyAsync(x =>
                    (rule.MinScore <= x.MaxScore && rule.MaxScore > x.MinScore));

                if (overlap)
                  return  ServiceResponse.Fail( "Grade range overlaps with existing rule.");

                var duplicate = await _context.GradeRules
                    .AnyAsync(x => x.RuleName == rule.RuleName);

                if (duplicate)
                    return ServiceResponse.Fail( "Grade name already exists.");

                _context.GradeRules.Add(rule);
                await _context.SaveChangesAsync();

                return ServiceResponse.Ok( "Grade rule created successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse.Fail( ex.Message);
            }
        }

        public async Task<ServiceResponse> UpdateAsync(GradeRule rule)
        {
            try
            {
                if (rule.MinScore > rule.MaxScore)
                    return ServiceResponse.Fail("Invalid percentage range.");

                var overlap = await _context.GradeRules.AnyAsync(x =>
                    x.GradeRuleID != rule.GradeRuleID &&
                    (rule.MinScore <= x.MaxScore &&
                     rule.MaxScore >= x.MinScore));

                if (overlap)
                    ServiceResponse.Fail("Overlapping grade range detected.");

                _context.GradeRules.Update(rule);
                await _context.SaveChangesAsync();

                return ServiceResponse.Ok("Grade rule updated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse.Fail(ex.Message);
            }
        }


        public async Task<ServiceResponse> DeleteAsync(int id)
        {
            try
            {
                var gradeRule = await _context.GradeRules
                        .FirstOrDefaultAsync( x => x.GradeRuleID == id);

                if (gradeRule == null)
                    return ServiceResponse.Fail("Record not found.");

                _context.GradeRules.Remove(gradeRule);

                await _context.SaveChangesAsync();
                
                return ServiceResponse.Ok("Record deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting GradeRule {Id}", id);
                return ServiceResponse.Fail(ex.Message);
            }
        }


    }
}
