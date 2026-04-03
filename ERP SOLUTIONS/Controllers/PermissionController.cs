
using ERP_SOLUTIONS.Models.ViewModels;
using ERP_SOLUTIONS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ERP_SOLUTIONS.Controllers
{
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
                TempData["SuccessMessage"] = "Selected Permissions assigned successfully!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Selected Permissions unable to assign.";
            }

            return RedirectToAction("Index", new { roleId = model.RoleId });
        }

    }
}
