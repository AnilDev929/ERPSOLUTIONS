using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;

namespace SchoolERP.Controllers
{
    [Authorize(Roles = "Admin")] // 🔒 IMPORTANT
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
            var students =
                    from s in _context.Students
                    join se in _context.StudentEnrollments on s.StudentID equals se.StudentId
                    //join cs in _context.ClassSections on s.ClassSectionId equals cs.Id
                    join c in _context.Classes on se.ClassId equals c.ClassId
                    join sec in _context.Sections on se.SectionId equals sec.SectionId
                    join u in _context.Users on s.UserID equals u.UserID
                    select new StudentLockViewModel
                    {
                        StudentId = s.StudentID,
                        UserID = s.UserID,
                        Name = s.StudentName,
                        ClassName = c.ClassName,
                        SectionName = sec.SectionName,  
                        LastLogin = u.LastLoginAt.HasValue ? u.LastLoginAt.Value.ToString("dd MMM yyyy hh:mm:ss tt") : "",
                        FailedCount = u.FailedLoginCount,
                        IsLocked = u.IsLocked.Value
                    };

            return View(students.ToList());
        }

        public IActionResult TeacherLock()
        {
            var teachers =
                    from t in _context.Teachers
                    join u in _context.Users on t.UserID equals u.UserID
                    select new TeacherLockDTO
                    {
                        TeacherID = t.TeacherID,
                        UserID = u.UserID,
                        UserName = u.UserName,
                        FullName = t.FullName,
                        MobileNo = t.Phone,
                        EmailID = t.EmailID,
                        LastLogin = u.LastLoginAt.HasValue ? u.LastLoginAt.Value.ToString("dd MMM yyyy hh:mm:ss tt") : "",
                        FailedCount = u.FailedLoginCount,
                        IsLocked = u.IsLocked.Value
                    };

            return View(teachers.ToList());
        }

        public IActionResult ParentLock(int? parentId)
        {
            var parents = 
                (from p in _context.Parents 
                 join ps in _context.ParentStudent on p.ParentId equals ps.ParentId 
                 join s in _context.Students on ps.StudentId equals s.StudentID 
                 join se in _context.StudentEnrollments on s.StudentID equals se.StudentId
                 join c in _context.Classes on se.ClassId equals c.ClassId 
                 join sec in _context.Sections on se.SectionId equals sec.SectionId 
                 where se.Status == "Active"
                 select new 
                 { 
                     p.ParentId, 
                     ParentName = p.FatherName, 
                     p.Phone, p.ParentEmail, p.IsLocked, 
                     p.LastLoginAt, 
                     //p.LockReason, 
                     StudentId = s.StudentID, 
                     StudentName = s.StudentName, 
                     ClassName = c.ClassName, 
                     SectionName = sec.SectionName 
                 }).ToList(); 
            
            var model = parents.GroupBy(x => x.ParentId)
                .Select(g => new ParentUnlockViewModel 
                { 
                    ParentId = g.Key, 
                    ParentName = g.First().ParentName, 
                    MobileNo = g.First().Phone, 
                    Email = g.First().ParentEmail, 
                    IsLocked = g.First().IsLocked, 
                    LastLoginAt = g.First().LastLoginAt, 
                    //LockReason = g.First().LockReason, 
                    Children = g.Select(x => 
                    new StudentChildViewModel 
                    { 
                        StudentId = x.StudentId, 
                        StudentName = x.StudentName, 
                        ClassName = x.ClassName, 
                        SectionName = x.SectionName 
                    }).ToList() 
                }).ToList(); 
            
            ViewBag.SelectedParent = parentId != null ? model.FirstOrDefault(x => x.ParentId == parentId)  : null; 
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockParent(int parentId) 
        { 
            var parent = await _context.Parents.FirstOrDefaultAsync(x => x.ParentId == parentId);

            if (parent == null)
            {
                TempData["Error"] = "Parent detail not found.";
                return RedirectToAction("Index");
            }

            parent.IsLocked = false; 
            //parent.LockReason = null; 
            await _context.SaveChangesAsync(); 
            
            TempData["Success"] = "Parent access unlocked successfully.";
            return RedirectToAction("ParentLock", new { parentId }); 
        }


        // Toggle Lock/Unlock
        [HttpPost]
        public IActionResult ToggleLock(int userId)
        {
            var user = _context.Users.SingleOrDefault(x => x.UserID == userId);
            //var student = _context.Students.Find(userId);
            if (user != null)
            {
                user.IsLocked = !user.IsLocked;
                _context.SaveChanges();
                if (user.IsLocked.Value)
                {
                    TempData["Success"] = "Student has been locked successfully.";
                }
                else
                {
                    TempData["Warning"] = "Student has been unlocked successfully.";
                }
            }

            return RedirectToAction("StudentLock");
        }

        // Toggle Lock/Unlock
        [HttpPost]
        public IActionResult TeacherToggleLock(int userId)
        {
            var user = _context.Users.SingleOrDefault(x => x.UserID == userId);
            //var teacher = _context.Teachers.Find(teacherID);
            if (user != null)
            {
                user.IsLocked = !user.IsLocked;
                _context.SaveChanges();

                if (user.IsLocked.Value)
                {
                    TempData["Success"] = "Student has been locked successfully.";
                }
                else
                {
                    TempData["Warning"] = "Student has been unlocked successfully.";
                }
            }

            return RedirectToAction("StudentLock");
        }
    }
}
