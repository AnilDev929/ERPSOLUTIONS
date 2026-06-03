
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SchoolERP.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PermissionController : Controller
    {
        private readonly IPermissionService _service;

        public PermissionController(IPermissionService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(int roleId)
        {
            ViewBag.Roles = await _service.GetAllRolesAsync();

            if (roleId == 0)
                return View(new RolePermissionViewModel());

            var model = await _service.GetRolePermissionsAsync(roleId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SavePermissions(RolePermissionViewModel model)
        {
            try
            {
                await _service.SaveRolePermissionsAsync(model);
                TempData["Success"] = "Selected Permissions assigned successfully!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Selected Permissions unable to assign.";
            }

            return RedirectToAction("Index", new { roleId = model.RoleId });
        }

    }
}
