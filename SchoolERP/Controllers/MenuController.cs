using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Implementations;

namespace SchoolERP.Controllers
{
    [Authorize(Roles = "Admin, SuperAdmin")] // 🔒 IMPORTANT
    public class MenuController : Controller
    {
        private readonly MenuService _menuService;

        public MenuController(MenuService menuService)
        {
            _menuService = menuService;
        }

        // GET
        public IActionResult CreateMenu()
        {
            MenuPageVM all = _menuService.GetAllMenus();
            return View(all);
        }

        // POST
        [HttpPost]
        //public IActionResult CreateMenu(MenuSectionVM vm)
        public IActionResult CreateMenu(MenuPageVM vm)
        {
            if(vm.NewItems == null  || vm.NewItems.Count() == 0)
            {
                TempData["Error"] = "Sub-items are not added.";
                MenuPageVM all = _menuService.GetAllMenus();
                return View(all);
            }

            if (!ModelState.IsValid)
            {
                MenuPageVM all = _menuService.GetAllMenus();
                return View(all);
            }

            string message = _menuService.CreateMenu(vm);
            TempData["Success"] = message;
            return RedirectToAction("CreateMenu");
        }

        [HttpGet]
        public IActionResult GetSections(string term)
        {
            var result =  _menuService.SearchSections(term);
            return Json(result);
        }

        public IActionResult ManageMenu()
        {
            List<MenuSectionDTO> data = _menuService.ManageMenu();

            return View(data);
        }


        [HttpPost]
        public IActionResult ToggleSection(int id, bool isActive)
        {
            var result = _menuService.ToggleSection(id, isActive);


            return Ok();
        }

        [HttpPost]
        public IActionResult ToggleItem(int id, bool isActive)
        {
            var result = _menuService.ToggleItem(id, isActive);
            return Ok();
        }
    }
}
