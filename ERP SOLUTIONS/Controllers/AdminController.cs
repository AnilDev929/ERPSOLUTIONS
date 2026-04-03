using ERP_SOLUTIONS.Data;
using ERP_SOLUTIONS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP_SOLUTIONS.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Profile()
        {
            var roleId = Convert.ToInt32(User.FindFirst("RoleId")?.Value);

            var menuItems = await _context.MenuItems
               .Include(m => m.MenuSection)
               .Where(m => m.IsActive)
               .ToListAsync();

            var existingPermissions = await _context.RoleMenuAccess
                .Where(r => r.RoleID == roleId)
                .ToListAsync();

            // ⚡ Optimize lookup
            var permissionDict = existingPermissions.ToDictionary(p => p.MenuItemID);

            var model = new RolePermissionViewModel
            {
                RoleId = roleId,

                Sections = menuItems
                    .GroupBy(m => m.MenuSection.Title)
                    .Select(sectionGroup => new SectionPermission
                    {
                        SectionTitle = sectionGroup.Key,

                        Items = sectionGroup.Select(m =>
                        {
                            permissionDict.TryGetValue(m.Id, out var perm);

                            return new MenuPermissionItem
                            {
                                MenuItemId = m.Id,
                                MenuTitle = m.Title,

                                CanView = perm?.CanView ?? false,
                                CanCreate = perm?.CanCreate ?? false,
                                CanEdit = perm?.CanEdit ?? false,
                                CanDelete = perm?.CanDelete ?? false
                            };
                        }).ToList()
                    })
                    .ToList()
            };

            //var data = (from rma in _context.RoleMenuAccess
            //            join m in _context.MenuItems
            //            on rma.MenuItemID equals m.Id
            //            where rma.RoleID == roleId
            //            select new MenuPermissionItem
            //            {
            //                MenuItemId = m.Id,
            //                MenuTitle = m.Title,

            //                CanView = rma.CanView,
            //                CanCreate = rma.CanCreate,
            //                CanEdit = rma.CanEdit,
            //                CanDelete = rma.CanDelete
            //            }).ToList();

            //var sections = data
            //    .GroupBy(x => x.MenuTitle)
            //    .Select(g => new SectionPermission
            //    {
            //        SectionTitle = g.Key,
            //        Items = g.ToList()
            //    }).ToList();

            //var model = new RolePermissionViewModel
            //{
            //    RoleId = roleId,
            //    Sections = sections
            //};

            return View(model);
        }

        // Display students
        public IActionResult StudentLock()
        {
            var students = _context.Students
                .Select(s => new StudentLockViewModel
                {
                    StudentId = s.StudentID,
                    Name = s.StudentName,
                    ClassName = s.Classes.ClassName,
                    //SectionName = s.Section.SectionName,
                    IsLocked = s.IsLocked
                })
                .ToList();

            return View(students);
        }

        // Toggle Lock/Unlock
        [HttpPost]
        public IActionResult ToggleLock(int studentId)
        {
            var student = _context.Students.Find(studentId);
            if (student != null)
            {
                student.IsLocked = !student.IsLocked;
                _context.SaveChanges();
            }

            return RedirectToAction("StudentLock");
        }
    }
}
