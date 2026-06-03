using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;

namespace SchoolERP.Services.Implementations
{
    public class MenuService
    {
        private readonly IMemoryCache _cache;
        private readonly AppDbContext _context;

        public MenuService(IMemoryCache cache, AppDbContext context)
        {
            _cache = cache;
            _context = context;
        }

        public MenuPageVM GetAllMenus()
        {
            var sections = _context.MenuSections
                .Include(s => s.Items)
                .Select(s => new MenuSectionVM
                {
                    Id = s.Id,
                    Title = s.Title,
                    Icon = s.Icon,
                    Color = s.Color,
                    Subtitle = Convert.ToString(s.Subtitle),
                    IsActive = s.IsActive,
                    DisplayOrder = s.DisplayOrder,
                        ExistingItems = s.Items
                        .OrderBy(i => i.DisplayOrder)   // ✅ ORDER ITEMS HERE
                        .Select(i => new MenuItemDTO
                        {
                            Id = i.Id,
                            Title = i.Title,
                            Subtitle = Convert.ToString(i.Description),
                            Icon = i.Icon,
                            Url = i.Url,
                            DisplayOrder = i.DisplayOrder,
                            IsActive = i.IsActive
                        }).ToList()
                })
                .OrderBy(s => s.DisplayOrder)   // ✅ ORDER SECTIONS HERE
                .ToList();

            var vm = new MenuPageVM
            {
                ExistingSections = sections
            };

            return vm;
        }

        public List<MenuSectionDTO> SearchSections(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<MenuSectionDTO>();

            return _context.MenuSections
                .Where(x => x.Title.Contains(term))
                .Select(x => new MenuSectionDTO
                {
                    Id = x.Id,
                    Title = x.Title,
                    Icon = x.Icon,
                    Color = x.Color,
                    IsActive = x.IsActive,
                    Subtitle = x.Subtitle
                })
                .Take(10)
                .ToList();
        }

        public List<MenuSection> GetActiveMenus()
        {
            return _cache.GetOrCreate("ActiveMenus", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                return _context.MenuSections
                    .Where(s => s.IsActive)
                    .Include(s => s.Items.Where(i => i.IsActive))
                    .ToList();
            });
        }

        /*
         must clear the cache when: ( _cache.Remove("ActiveMenus"))
         1. ✅ 1. Add / Create
         2. ✅ 2. Update
         3. ✅ 3. Delete
         */
        public void AddSection(MenuSection section)
        {
            _context.MenuSections.Add(section);
            _context.SaveChanges();

            // 🔥 Clear cache after change
            _cache.Remove("ActiveMenus");
        }

        public void UpdateSection(MenuSection section)
        {
            _context.MenuSections.Update(section);
            _context.SaveChanges();

            _cache.Remove("ActiveMenus");
        }

        public void DeleteSection(int id)
        {
            var section = _context.MenuSections.Find(id);
            if (section != null)
            {
                _context.MenuSections.Remove(section);
                _context.SaveChanges();

                _cache.Remove("ActiveMenus");
            }
        }

        //public string CreateMenu(MenuSectionVM vm)
        //{
        //    using var transaction = _context.Database.BeginTransaction();
        //    try
        //    {
        //        // ✅ 1. Check if section exists
        //        var section = _context.MenuSections
        //            .FirstOrDefault(s => s.Title.ToLower() == vm.Title.ToLower());

        //        // ✅ 2. If NOT exists → create
        //        if (section == null)
        //        {
        //            //return "Section already exists.";

        //            section = new MenuSection
        //            {
        //                Title = vm.Title,
        //                Icon = vm.Icon,
        //                Color = vm.Color,
        //                Subtitle = vm.Subtitle,
        //                IsActive = vm.IsActive
        //            };

        //            _context.MenuSections.Add(section);
        //            _context.SaveChanges();
        //        }


        //        // ✅ 2. Remove duplicate items from request (same title)
        //        var uniqueItems = vm.Items
        //            .GroupBy(i => i.Title?.Trim().ToLower())
        //            .Select(g => g.First())
        //            .ToList();

        //        // 🔹 4. Add only non-existing items
        //        foreach (var item in vm.Items)
        //        {
        //            // ✅ 3. Duplicate check in DB (same section + title)
        //            bool itemExists = _context.MenuItems.Any(i =>
        //                i.SectionId == section.Id &&
        //                i.Title == item.Title);

        //            if (itemExists)
        //                continue; // skip duplicate

        //            var menuItem = new MenuItem
        //            {
        //                SectionId = section.Id,
        //                Title = item.Title,
        //                Description = item.Description,
        //                Icon = item.Icon,
        //                Url = item.Url,
        //                IsActive = item.IsActive
        //            };

        //            _context.MenuItems.Add(menuItem);
        //        }

        //        _context.SaveChanges();

        //        // ✅ Commit transaction
        //        transaction.Commit();

        //        // 🔥 Clear cache
        //        _cache.Remove("ActiveMenus");

        //        return "Menu added.";
        //    }
        //    catch (Exception ex)
        //    {
        //        // ❌ Rollback everything if error
        //        transaction.Rollback();
        //        return ex.Message;
        //    }
        //}

        public async Task<List<MenuSectionDTO>> GetMenuCachedAsync(int roleId)
        {
            var cacheKey = $"menu_role_{roleId}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                return await GetMenuByRoleAsync(roleId);
            });
        }

        public async Task<List<MenuSectionDTO>> GetMenuByRoleAsync(int roleId)
        {

            var data = await (
                from ms in _context.MenuSections
                join mi in _context.MenuItems on ms.Id equals mi.SectionId
                join rma in _context.RoleMenuAccess on mi.Id equals rma.MenuItemID
                where rma.RoleID == roleId
                      && rma.CanView
                      && ms.IsActive
                      && mi.IsActive
                orderby ms.Id
                select new
                {
                    SectionId = ms.Id,
                    SectionTitle = ms.Title,
                    SectionIcon = ms.Icon,
                    ms.Color,

                    MenuItemId = mi.Id,
                    MenuItemTitle = mi.Title,
                    MenuItemIcon = mi.Icon,
                    mi.Url
                }
            ).ToListAsync();

            var result = data
                .GroupBy(x => new
                {
                    x.SectionId,
                    x.SectionTitle,
                    x.SectionIcon,
                    x.Color
                })
                .Select(g => new MenuSectionDTO
                {
                    Id = g.Key.SectionId,
                    Title = g.Key.SectionTitle,
                    Icon = g.Key.SectionIcon,
                    Color = g.Key.Color,

                    Items = g.Select(i => new MenuItemDTO
                    {
                        Id = i.MenuItemId,
                        Title = i.MenuItemTitle,
                        Icon = i.MenuItemIcon,
                        Url = i.Url
                    }).ToList()
                })
                .ToList();

            return result;
        }


        public string CreateMenu(MenuPageVM vm)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // ✅ 1. Check if section exists
                var section = _context.MenuSections
                    .FirstOrDefault(s => s.Title.ToLower() == vm.NewSection.Title.ToLower());

                // ✅ 2. If NOT exists → create
                if (section == null)
                {
                    //return "Section already exists.";

                    section = new MenuSection
                    {
                        Title = vm.NewSection.Title,
                        Icon = vm.NewSection.Icon,
                        Color = vm.NewSection.Color,
                        Subtitle = vm.NewSection.Subtitle,
                        IsActive = vm.NewSection.IsActive
                    };

                    _context.MenuSections.Add(section);
                    _context.SaveChanges();
                }


                //// ✅ 2. Remove duplicate items from request (same title)
                //var uniqueItems = vm.NewSection.Items
                //    .GroupBy(i => i.Title?.Trim().ToLower())
                //    .Select(g => g.First())
                //    .ToList();

                // 🔹 4. Add only non-existing items
                foreach (var item in vm.NewItems)
                {
                    // ✅ 3. Duplicate check in DB (same section + title)
                    bool itemExists = _context.MenuItems.Any(i =>
                        i.SectionId == section.Id &&
                        i.Title == item.Title);

                    if (itemExists)
                        continue; // skip duplicate

                    var menuItem = new MenuItem
                    {
                        SectionId = section.Id,
                        Title = item.Title,
                        Description = item.Subtitle,
                        Icon = item.Icon,
                        Url = item.Url,
                        IsActive = item.IsActive
                    };

                    _context.MenuItems.Add(menuItem);
                }

                _context.SaveChanges();

                // ✅ Commit transaction
                transaction.Commit();

                // 🔥 Clear cache
                _cache.Remove("ActiveMenus");

                return "Menu added successfully.";
            }
            catch (Exception ex)
            {
                // ❌ Rollback everything if error
                transaction.Rollback();
                return ex.Message;
            }
        }


        public List<MenuSectionDTO> ManageMenu()
        {
            var data = _context.MenuSections
                .Select(s => new MenuSectionDTO
                {
                    Id = s.Id,
                    Title = s.Title,
                    IsActive = s.IsActive,
                    Subtitle = s.Subtitle,
                    Icon = s.Icon,
                    DisplayOrder = s.DisplayOrder,
                    ComponentName = s.ComponentName,
                    IsComponent = s.IsComponent,
                    Items = _context.MenuItems
                        .Where(i => i.SectionId == s.Id)
                        .Select(i => new MenuItemDTO
                        {
                            Id = i.Id,
                            Title = i.Title,
                            Icon = i.Icon,
                            //Subtitle = i.Description,
                            IsActive = i.IsActive
                        }).ToList()
                }).ToList();

            return data;
        }

        public bool ToggleSection(int id, bool isActive)
        {
            var section = _context.MenuSections.Find(id);
            if (section == null) return false;

            section.IsActive = isActive;

            // If section is disabled → disable all items
            if (!isActive)
            {
                var items = _context.MenuItems
                                    .Where(x => x.SectionId == id)
                                    .ToList();

                foreach (var item in items)
                {
                    item.IsActive = false;
                }
            }

            _context.SaveChanges();
            return true;
        }

        public bool ToggleItem(int id, bool isActive)
        {
            var item = _context.MenuItems.Find(id);
            if (item == null) return false;

            if (isActive)
            {
                var section = _context.MenuSections.Find(item.SectionId);
                if (section != null && !section.IsActive)
                    return false; // prevent enabling item if section is OFF

                //return (false, "Cannot enable item because its section is disabled.");
            }

            item.IsActive = isActive;
            _context.SaveChanges();

            return true;
        }
    }
}
