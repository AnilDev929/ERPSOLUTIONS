using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.Entities
{
    public class Role
    {
        public int RoleID { get; set; }

        [StringLength(30)]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public List<RoleMenuAccess> RoleMenuAccesses { get; set; } = new List<RoleMenuAccess>();

        // Navigation property to link users
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public Role() { }
    }
}
