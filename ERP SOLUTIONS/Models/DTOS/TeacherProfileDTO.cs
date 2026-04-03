using ERP_SOLUTIONS.Enum;
using System.ComponentModel.DataAnnotations;

namespace ERP_SOLUTIONS.Models.DTOS
{
    public class TeacherProfileDTO
    {
        // 🧾 Personal Info
        public string EncryptedId { get; set; }
        public int TeacherID { get; set; }

        [Display(Name = "Full Name")]
        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; }

        public string Designation { get; set; }

        [Display(Name = "Gender")]
        public string GenderName { get; set; } // Read-only for teacher

        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; } // Read-only for teacher

        [Display(Name = "Blood Group")]
        public string BloodGroup { get; set; }

        // 👨‍👩‍👧 Family Info
        [Display(Name = "Father Name")]
        public string FatherName { get; set; }

        [Display(Name = "Mother Name")]
        public string MotherName { get; set; }

        [Display(Name = "Married")]
        public bool IsMarried { get; set; }

        [Display(Name = "Spouse Name")]
        public string SpouseName { get; set; }

        [Display(Name = "Spouse Contact")]
        public string SpouseContact { get; set; }

        // 📞 Contact Info
        [Display(Name = "Phone")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string EmailID { get; set; }

        [Display(Name = "Emergency Contact")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string EmergencyContact { get; set; }

        [Display(Name = "Present Address")]
        public string PresentAddress { get; set; }

        [Display(Name = "Permanent Address")]
        public string PermanentAddress { get; set; }

        // 🎓 Professional Info
        [Display(Name = "Qualification")]
        public string Qualification { get; set; }

        [Display(Name = "Specialization")]
        public string Specialization { get; set; }

        [Display(Name = "Experience (years)")]
        public decimal? Experience { get; set; }

        public string? StatusRemark { get; set; }
        public string? StatusUpdatedOn { get; set; }
        public string? JoiningDate { get; set; }
        public string Aadhaar { get; set; } = string.Empty;
        public TeacherStatus Status { get; set; } = TeacherStatus.Active;
    }
}
