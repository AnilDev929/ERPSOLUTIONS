namespace SchoolERP.Models.ViewModels
{
    public class MenuPermissionItem
    {
        public int MenuItemId { get; set; }
        public string SectionTitle { get; set; }
        public string MenuTitle { get; set; }

        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class SectionPermission
    {
        public string SectionTitle { get; set; }
        public List<MenuPermissionItem> Items { get; set; } = new();
    }

    public class RolePermissionViewModel
    {
        public int RoleId { get; set; }
        //public List<MenuPermissionItem> Permissions { get; set; }
        public List<SectionPermission> Sections { get; set; } = new();
    }
}

