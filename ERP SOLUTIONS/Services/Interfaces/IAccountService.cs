using ERP_SOLUTIONS.Models.DTOS;
using ERP_SOLUTIONS.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ERP_SOLUTIONS.Services.Interfaces
{
    public interface IAccountService
    {
        List<SelectListItem> GetRolesDropdown();

        Task<(bool Success, string Message)> LoginAsync(string username, string password, int roleid);

        Task<UserInfoDTO> GetUserInfoAsync(string username);

        Task<ChangePasswordResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
