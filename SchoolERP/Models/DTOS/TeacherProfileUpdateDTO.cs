using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.DTOS
{
    public class TeacherProfileUpdateDTO
    {
        public int Id { get; set; }

        // 👤 Basic Info
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; }
        // ℹ️ Optional Personal Info

        [Required]
        public int GenderID { get; set; }
        public string BloodGroup { get; set; }
        public string Aadhaar { get; set; }
        // 📞 Contact Info
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string EmergencyContact { get; set; }

        // 📧 Email (should require verification)
        [Required]
        public string Email { get; set; }

        // 🏠 Address
        [Required]
        public string PermanentAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string PostalCode { get; set; }
      
        public string EmploymentType { get; set; }
        public bool IsMarried { get; set; } = false;
        public string? SpouseName { get; set; }
        public string? SpouseContact { get; set; }
        public int? ProfileCompletion { get; set; }
        public DateTime JoiningDate { set; get; }
        public string? Designation {  get; set; }

        public IFormFile? ProfileImageInput { get; set; }
        // 🖼 Profile
        public string? ProfileImageUrl { get; set; }
    }
}
