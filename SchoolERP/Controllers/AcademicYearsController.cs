using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Implementations;
using SchoolERP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SchoolERP.Controllers
{
    [Authorize(Roles = "Admin")] // 🔒 IMPORTANT
    public class AcademicYearsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAcademicYearService yearService;

        public AcademicYearsController(AppDbContext context, IAcademicYearService academicYear)
        {
            _context = context;
            yearService = academicYear;
        }

        // LIST + FILTER
        public IActionResult Index(string search, bool? status)
        {
            var data = yearService.GetAllAsync(); //   _context.AcademicYears.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x => x.YearName.Contains(search));
            }

            if (status.HasValue)
            {
                data = data.Where(x => x.IsActive == status.Value);
            }

            return View(data.OrderByDescending(x => x.YearStart).ToList());
        }

        // CREATE GET
        public IActionResult Create()
        {
            return View();
        }

        // HELPER: Check for duplicate
        private async Task<bool> IsDuplicateAsync(string yearName, int id = 0)
        {
            return await _context.AcademicYears
                .AnyAsync(x => x.YearName.ToLower() == yearName.ToLower() && x.AcademicYearID != id);
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcademicYear model)
        {
            if (ModelState.IsValid)
            {
                // 1. Validate date range
                if (model.YearEnd <= model.YearStart)
                {
                    ModelState.AddModelError("", "End date must be greater than Start date.");
                }

                string status = await yearService.AddAsync(model);

                if (status == "duplicate")
                {
                    ModelState.AddModelError("YearName", $"Academic Year '{model.YearName}' already exists.");
                }
                if (status == "Error")
                {
                    return View(model);
                }

                #region Old Code
                //// 2. Check for duplicate YearName
                //bool isDuplicate = await _context.AcademicYears
                //    .AnyAsync(x => x.YearName.ToLower() == model.YearName.ToLower());
                //if (isDuplicate)
                //{
                //    ModelState.AddModelError("YearName", $"Academic Year '{model.YearName}' already exists.");
                //}

                //// 3. Ensure only one active year
                //if (model.IsActive)
                //{
                //    var activeYears = await _context.AcademicYears
                //        .Where(x => x.IsActive)
                //        .ToListAsync();

                //    foreach (var item in activeYears)
                //    {
                //        item.IsActive = false;
                //    }
                //}

                //// 4. Save
                //_context.AcademicYears.Add(model);
                //await _context.SaveChangesAsync();
                #endregion

                //return RedirectToAction("Index");
                return RedirectToAction("Index", new
                {
                    search = "",
                    status = false
                });
            }

            // 5. Return to form if validation failed
            return View(model);
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var data = await yearService.GetByIdAsync(id);
            //var data = await _context.AcademicYears.FindAsync(id);
            if (data == null) { 
                TempData["Error"] = "Record not found";
                //return RedirectToAction("Index");
                return RedirectToAction("Index", new
                {
                    search = "",
                    status = false
                });
            }
            return View(data);
        }


        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AcademicYear model)
        {
            // Validate date range
            if (model.YearEnd <= model.YearStart)
            {
                ModelState.AddModelError("", "End date must be greater than Start date.");
            }

            // Check duplicate excluding current record
            if (await IsDuplicateAsync(model.YearName, model.AcademicYearID))
            {
                ModelState.AddModelError("YearName", $"Academic Year '{model.YearName}' already exists.");
            }

            if (ModelState.IsValid)
            {
                string result = await yearService.UpdateAsync(model);
                if(result == "exists")
                {
                    return View(model);
                }
                if (result == "notfound")
                {
                    return View(model);
                }
                if (result == "updated")
                {
                    //return RedirectToAction("Index");
                    return RedirectToAction("Index", new
                    {
                        search = "",
                        status = false
                    });
                }

                //// Ensure only one active year
                //if (model.IsActive)
                //{
                //    var activeYears = await _context.AcademicYears
                //        .Where(x => x.IsActive && x.AcademicYearID != model.AcademicYearID)
                //        .ToListAsync();

                //    foreach (var item in activeYears)
                //    {
                //        item.IsActive = false;
                //    }
                //}

                //_context.AcademicYears.Update(model);
                //await _context.SaveChangesAsync();

                //return RedirectToAction("MarksEntry");
            }

            return View(model);
        }


        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await yearService.DeleteAsync(id);

            //var data = await _context.AcademicYears.FindAsync(id);
            //if (data != null)
            //{
            //    _context.AcademicYears.Remove(data);
            //    await _context.SaveChangesAsync();
            //}

            //return RedirectToAction("Index");
            return RedirectToAction("Index", new
            {
                search = "",
                status = false
            });
        }
    }
}
