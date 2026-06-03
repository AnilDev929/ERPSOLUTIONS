using Microsoft.AspNetCore.Mvc;
using SchoolERP.Data;
using SchoolERP.Models.Entities;

namespace SchoolERP.Controllers
{
    public class SettingsController : Controller
    {
        private readonly AppDbContext _context;

        public SettingsController(AppDbContext context) {
            _context = context;
        }

        public IActionResult Index()
        {
            var settings = _context.MenuSettings.ToList();
            return View(settings);
        }


        [HttpPost]
        public ActionResult Save(MenuSetting model)
        {
            var setting = _context.MenuSettings.Find(model.Id);

            if (setting != null)
            {
                setting.IsActive = model.IsActive;
                setting.StartDate = model.StartDate;
                setting.EndDate = model.EndDate;

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }


    }
}
