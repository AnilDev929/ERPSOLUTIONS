using ERP_SOLUTIONS.Models.DTOS;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ERP_SOLUTIONS.Services.Interfaces
{
    public interface IAccountService
    {
        List<SelectListItem> GetRolesDropdown();

        Task<(bool Success, string Message)> LoginAsync(string username, string password, int roleid);

        Task<UserInfoDTO> GetUserInfoAsync(string username);
    }
}
