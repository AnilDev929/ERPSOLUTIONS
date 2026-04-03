
using ERP_SOLUTIONS.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ERP_SOLUTIONS.Models.Entities
{
    public class Teacher
    {
        public int TeacherID { get; set; }
        public int UserID { get; set; }

        [Required(ErrorMessage = "Designation is required.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Designation name can contain only alphabets and spaces.")]
        public string Designation { get; set; }

        [DisplayName("Full Name")]
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Full name can contain only alphabets and spaces.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please select gender.")]
        public int GenderID { get; set; }

        [DisplayName("DOB")]
        [Required(ErrorMessage = "Date of birth is required.")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Blood group is required.")]
        public string BloodGroup { get; set; }

        [DisplayName("Father Name")]
        [Required(ErrorMessage = "Father's name is required.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Father name can contain only alphabets and spaces.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Father's name must be between 3 and 100 characters.")]

        public string FatherName { get; set; }

        [DisplayName("Mother Name")]
        [Required(ErrorMessage = "Mother's name is required.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Mother name can contain only alphabets and spaces.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Mother's name must be between 3 and 100 characters.")]
        public string MotherName { get; set; }

        [DisplayName("Emergency Contact")]
        [Required(ErrorMessage = "Emergency contact is required.")]
        public string? EmergencyContact { get; set; }

        [DisplayName("Is Married")]
        public bool IsMarried { get; set; }

        [DisplayName("Spouse Name")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Spouce name can contain only alphabets and spaces.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Spouce name must be between 3 and 100 characters.")]
        public string? SpouseName { get; set; }

        [DisplayName("Spouse Contact")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit Indian mobile number.")]
        public string? SpouseContact { get; set; }

        [DisplayName("Aadhaar Number")]
        [Required(ErrorMessage = "Aadhaar number is required.")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Aadhaar must be 16 digits.")]
        public string Aadhaar { get; set; }

        [DisplayName("Mobile Number")]
        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be 10 digits.")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit Indian mobile number.")]
        public string Phone { get; set; } = string.Empty;

        [DisplayName("Email Address")]
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string EmailID { get; set; }

        [DisplayName("Present Address")]
        [Required(ErrorMessage = "Present address is required.")]
        [StringLength(250, MinimumLength = 10, ErrorMessage = "Address must be between 10 and 250 characters.")]
        public string PresentAddress { get; set; }

        [DisplayName("Permanent Address")]
        [Required(ErrorMessage = "Permanent address is required.")]
        [StringLength(250, MinimumLength = 10, ErrorMessage = "Address must be between 10 and 250 characters.")]
        public string PermanentAddress { get; set; }

        [Required(ErrorMessage = "Qualification is required.")]
        [StringLength(100, ErrorMessage = "Qualification cannot exceed 100 characters.")]
        public string Qualification { get; set; }

        [Required(ErrorMessage = "Specialization is required.")]
        [StringLength(100, ErrorMessage = "Specialization cannot exceed 100 characters.")]
        public string Specialization { get; set; }

        [Required(ErrorMessage = "Experience is required.")]
        [Range(1, 50, ErrorMessage = "Experience must be between 1 and 50 years.")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Enter valid experience (up to 2 decimal places).")]
        public decimal? Experience { get; set; }

        [DisplayName("Joining Date")]
        [Required(ErrorMessage = "Joining date is required.")]
        public DateTime? JoiningDate { get; set; }

        [DisplayName("Last Working Date")]
        public DateTime? LastWorkingDate { get; set; }

        public TeacherStatus Status { get; set; } = TeacherStatus.Active;
        public string? StatusRemark { get; set; }
        public DateTime? StatusUpdatedOn { get; set; }

        //public ICollection<LeaveRequest> LeaveRequests { get; set; }
    }


}
