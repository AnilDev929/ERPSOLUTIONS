using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SchoolERP.Services.Implementations
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;

        public PermissionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        //public async Task<RolePermissionViewModel> GetRolePermissionsAsync(int roleId)
        //{
        //    var menuItems = await _context.MenuItems
        //        .Include(m => m.MenuSection)
        //        .Where(m => m.Status)
        //        .ToListAsync();

        //    var existingPermissions = await _context.RoleMenuAccess
        //        .Where(r => r.RoleID == roleId)
        //        .ToListAsync();

        //    var model = new RolePermissionViewModel
        //    {
        //        RoleId = roleId,
        //        Permissions = menuItems.Select(m =>
        //        {
        //            var perm = existingPermissions.FirstOrDefault(p => p.MenuItemID == m.Id);

        //            return new MenuPermissionItem
        //            {
        //                MenuItemId = m.Id,
        //                SectionTitle = m.MenuSection.Title,
        //                MenuTitle = m.Title,
        //                CanView = perm?.CanView ?? false,
        //                CanCreate = perm?.CanCreate ?? false,
        //                CanEdit = perm?.CanEdit ?? false,
        //                CanDelete = perm?.CanDelete ?? false
        //            };
        //        }).ToList()
        //    };

        //    return model;
        //}


        // ✅ GET DATA (FOR UI)
        public async Task<RolePermissionViewModel> GetRolePermissionsAsync(int roleId)
        {
            var menuItems = await _context.MenuItems
                .Include(m => m.MenuSection)
                .Where(m => m.IsActive)
                .ToListAsync();

            var existingPermissions = await _context.RoleMenuAccess
                .Where(r => r.RoleID == roleId)
                .ToListAsync();

            // ⚡ Optimize lookup
            var permissionDict = existingPermissions
                .ToDictionary(p => p.MenuItemID);

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

            return model;
        }


        //public async Task SaveRolePermissionsAsync(RolePermissionViewModel model)
        //{
        //    var existing = _context.RoleMenuAccess
        //        .Where(x => x.RoleID == model.RoleId);

        //    _context.RoleMenuAccess.RemoveRange(existing);

        //    var newPermissions = model.Permissions.Select(p => new RoleMenuAccess
        //    {
        //        RoleID = model.RoleId,
        //        MenuItemID = p.MenuItemId,
        //        CanView = p.CanView,
        //        CanCreate = p.CanCreate,
        //        CanEdit = p.CanEdit,
        //        CanDelete = p.CanDelete
        //    });

        //    await _context.RoleMenuAccess.AddRangeAsync(newPermissions);
        //    await _context.SaveChangesAsync();
        //}

        // ✅ SAVE DATA
        public async Task SaveRolePermissionsAsync(RolePermissionViewModel model)
        {
            var roleId = model.RoleId;

            // Get existing permissions
            var existingPermissions = await _context.RoleMenuAccess
                .Where(r => r.RoleID == roleId)
                .ToListAsync();

            // Flatten all items
            var allItems = model.Sections.SelectMany(s => s.Items).ToList();

            foreach (var item in allItems)
            {
                var existing = existingPermissions
                    .FirstOrDefault(x => x.MenuItemID == item.MenuItemId);

                if (existing != null)
                {
                    // 🔁 UPDATE
                    existing.CanView = item.CanView;
                    existing.CanCreate = item.CanCreate;
                    existing.CanEdit = item.CanEdit;
                    existing.CanDelete = item.CanDelete;
                }
                else
                {
                    // ➕ INSERT
                    var newPerm = new RoleMenuAccess
                    {
                        RoleID = roleId,
                        MenuItemID = item.MenuItemId,
                        CanView = item.CanView,
                        CanCreate = item.CanCreate,
                        CanEdit = item.CanEdit,
                        CanDelete = item.CanDelete
                    };

                    _context.RoleMenuAccess.Add(newPerm);
                }
            }

            await _context.SaveChangesAsync();
        }
    }

}
