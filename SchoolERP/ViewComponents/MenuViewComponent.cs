using Microsoft.AspNetCore.Mvc;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Implementations;
using System.Security.Claims;

namespace SchoolERP.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly MenuService _menuService;

        public MenuViewComponent(MenuService menuService)
        {
            _menuService = menuService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int roleId = Convert.ToInt32(UserClaimsPrincipal.FindFirst("RoleID")?.Value);
            var menu = await _menuService.GetMenuCachedAsync(roleId);

            return View(menu);
        }
    }
}
