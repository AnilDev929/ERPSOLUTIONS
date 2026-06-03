using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SchoolERP.Controllers
{
    [Authorize(Roles = "Admin")] // 🔒 IMPORTANT
    public class HostelController : Controller
    {
        private readonly IHostelService _hostelService;
        public HostelController(IHostelService hostelService)
        {
            _hostelService = hostelService;
        }

        // GET: List Page
        public async Task<ActionResult> Index()
        {
            var data = await _hostelService.GetAllHostelsAsync();
            return View(data);
        }

        // GET: Create Page
        public ActionResult Create()
        {
            var model = new HostelWithRoomsVM
            {
                Hostel = new Hostel(),
                Rooms = new List<RoomVM>()
            };

            return View(model);
        }

        // POST: Save Hostel + Rooms
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(HostelWithRoomsVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                foreach (var room in model.Rooms)
                {
                    // Extra safety validation
                    if (room.RoomNumber == "" || room.Capacity < 0 || room.Fee < 0)
                    {
                        TempData["Warning"] = "Invalid data detected.";
                        return RedirectToAction("Create", model);
                    }
                }

                await _hostelService.CreateHostelAsync(model);
                TempData["Success"] = "Hostel and Rooms created successfully!";
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                TempData["Error"] =  "Error: " + ex.Message;
                //return View(model);
                return RedirectToAction("Create", model);
            }
        }


        public async Task<ActionResult> Edit(int id)
        {
            var model = await _hostelService.GetHostelWithRoomsAsync(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(HostelWithRoomsVM model)
        {
            try
            {
                await _hostelService.UpdateHostelAsync(model);
                TempData["Success"] = "Updated successfully!";
                return RedirectToAction("Edit", new { id = model.Hostel.HostelId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }



    }
}
