using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace SchoolERP.Services.Interfaces
{
    public interface IAccountService
    {
        List<SelectListItem> GetRolesDropdown();

        Task<(bool Success, string Message)> LoginAsync(string username, string password, int roleid);
        Task<(int Success, string Message)> ParentLogin(LoginModel login);
        Task<User> GetParentDtl(int parentID);
        Task<UserInfoDTO> GetUserInfoAsync(string username);

        Task<ChangePasswordResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
