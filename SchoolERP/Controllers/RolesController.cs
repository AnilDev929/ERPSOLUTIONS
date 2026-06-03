using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace SchoolERP.Controllers
{
    [Authorize(Roles = "Admin")] // 🔒 IMPORTANT
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: /Roles
        public async Task<IActionResult> Index()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return View(roles); // uses Role list
        }

        // GET: /Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _roleService.CreateRoleAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(model);
        }

        // GET: /Roles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);

                if (role == null)
                {
                    //return NotFound(); // You can customize this too
                    return View("NotFound");
                }

                return View(role);
            }
            catch (Exception ex)
            {
                // Log error
                // _logger.LogError(ex, "Error loading role with ID {RoleID}", id);

                return RedirectToAction("Error", "Account");
            }
        }

        // POST: /Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Role model)
        {
            if (ModelState.IsValid)
            {
                var updatedRole = await _roleService.UpdateRoleAsync(model);
                if (updatedRole == null)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: /Roles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _roleService.DeleteRoleAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
