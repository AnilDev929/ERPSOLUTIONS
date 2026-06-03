using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SchoolERP.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RoleService> _logger;

        public RoleService(AppDbContext context, ILogger<RoleService> logger) { 
            _context = context;
            _logger = logger;
        }

        // READ (All)
        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        // READ (By Id)
        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            var role = await _context.Roles
                .Where(r => r.RoleID == id && r.IsActive)
                .FirstOrDefaultAsync();

            if (role == null)
            {
                _logger.LogWarning("Role with ID {RoleID} not found", id);
                return null;
            }

            return role;
        }

        // CREATE
        public async Task<Role> CreateRoleAsync(Role role)
        {
            // Validation example
            if (string.IsNullOrWhiteSpace(role.RoleName))
            {
                _logger.LogError("Role creation failed: RoleName is required");
                throw new ArgumentException("RoleName is required");
            }

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Role created with ID {RoleID}", role.RoleID);

            return role;
        }

        // UPDATE
        public async Task<Role?> UpdateRoleAsync(Role role)
        {
            var existingRole = await _context.Roles.FindAsync(role.RoleID);

            if (existingRole == null)
            {
                _logger.LogWarning("Role with ID {RoleID} not found for update", existingRole.RoleID);
                return null;
            }

            existingRole.RoleName = role.RoleName;
            existingRole.Description = role.Description;
            existingRole.IsActive = role.IsActive;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Role with ID {RoleID} updated", role.RoleID);
            return existingRole;
        }

        // DELETE
        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
                return false;

            if (role == null || role.IsActive)
            {
                _logger.LogWarning("Role with ID {RoleID} not found for deletion", id);
                return false;
            }

            role.IsActive = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Role with ID {RoleID} soft deleted", id);
            return true;
        }
    }
}
