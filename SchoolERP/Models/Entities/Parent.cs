using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Models.Entities
{
    public class Parent
    {
        [Key]
        public int ParentId { get; set; }

        [Required]
        [StringLength(150)]
        public string ParentEmail { get; set; }

        [Required]
        [StringLength(13)]
        public string Phone { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        public bool EmailVerified { get; set; } = false;

        public bool PhoneVerified { get; set; } = false;

        [StringLength(150)]
        public string FatherName { get; set; }

        [StringLength(12)]
        public string FatherAadhaar { get; set; }

        [StringLength(150)]
        public string MotherName { get; set; }

        [StringLength(12)]
        public string MotherAadhaar { get; set; }

        [StringLength(10)]
        public string EmergencyContact { get; set; }

        public string Category { get; set; }
        public string Religion { get; set; }

        [Required]
        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string State { get; set; }

        [StringLength(6)]
        public string Pincode { get; set; }

        [StringLength(100)]
        public string ParentOccupation { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AnnualIncome { get; set; }

        [StringLength(100)]
        public string CompanyName { get; set; }

        public bool ReceiveSms { get; set; } = false;

        public bool ReceiveEmail { get; set; } = false;

        public int? FailedLoginAttempts { get; set; }

        public bool IsLocked { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<ParentStudent>? ParentStudents { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? PreviousLoginAt { get; set; }

    }
}
