using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;

namespace SchoolERP.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<RolePermissionViewModel> GetRolePermissionsAsync(int roleId);
        Task SaveRolePermissionsAsync(RolePermissionViewModel model);
        Task<List<Role>> GetAllRolesAsync();
    }
}
