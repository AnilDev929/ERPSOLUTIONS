
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Models.Entities
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public string UserName { get; set; }
        public string? FullName { get; set; }
        public string? MobileNo { get; set; }

        public bool? IsLocked { get; set; } = false;
        public bool IsActive { get; set; } = true;

        //public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; } = null;
        public DateTime? PreviousLoginAt { get; set; } = null;
        public int FailedLoginCount { set; get; } = 0;

        public DateTime? LockoutUntil { get; set; } = null;

        // Navigation property
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public User() { }
    }
}
