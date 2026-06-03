namespace SchoolERP.Models.Entities
{
    public class RoleMenuAccess
    {
        public int Id { get; set; }

        public int RoleID { get; set; }
        public int MenuItemID { get; set; }


        public bool CanView { get; set; } = false;
        public bool CanCreate { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;

        public Role Role { get; set; }
        public MenuItem MenuItem { get; set; }
    }
}
