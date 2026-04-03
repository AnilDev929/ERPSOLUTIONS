using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_SOLUTIONS.Models.ViewModels
{
    public class StudentFormViewModel : IValidatableObject
    {
        // Student Info
        [Required(ErrorMessage = "Student Name is required.")]
        [StringLength(100)]
        public string StudentName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Select Gender")]
        public int GenderID { get; set; }

        [Required(ErrorMessage = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Address is missiing")]
        [StringLength(250)]
        public string PermanentAddress { get; set; }

        [Required(ErrorMessage = "Phone number is missing")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone must be 10 digits")]
        public string PhoneNo { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string EmailID { get; set; }

        [Required(ErrorMessage = "Blood group is missiing")]
        public string BloodGroup { get; set; }

        [Required(ErrorMessage = "Aadhaar Number is missiing")]
        [RegularExpression(@"^[0-9]{12}$", ErrorMessage = "Aadhaar must be 12 digits")]
        public string Aadhaar { get; set; }

        [RegularExpression(@"^$|^[0-9]{12}$", ErrorMessage = "APAAR ID must be 12 digits")]
        public string? ApaarID { get; set; }

        [Required(ErrorMessage = "Father Name is missing")]
        [StringLength(100)]
        public string FatherName { get; set; }

        [Required(ErrorMessage = "Mother Name is missing")]
        [StringLength(100)]
        public string MotherName { get; set; }

        [Required(ErrorMessage = "Mobile Number is missing")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Emergency contact must be 10 digits")]
        public string EmergencyContact { get; set; }

        [Required(ErrorMessage = "Father Aadhar Number is missing")]
        [RegularExpression(@"^[0-9]{12}$")]
        public string FatherAadhaar { get; set; }

        [Required(ErrorMessage = "Mother Aadhar Number is missing")]
        [RegularExpression(@"^[0-9]{12}$")]
        public string MotherAadhaar { get; set; }

        [Required]
        public int? AcademicYearID { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Select Class")]
        public int? ClassID { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Select Section")]
        public int? SectionID { get; set; }

        [NotMapped]
        [Range(1, int.MaxValue, ErrorMessage = "Select Class Section")]
        public int ClassSectionID { get; set; }

        public int? AdmissionYear { get; set; } = DateTime.Now.Year;

        [NotMapped]
        public string? RollNumber { get; set; }

        // Fees
        [NotMapped]
        public decimal TuitionFee { get; set; }
        [NotMapped]
        public decimal? LabFee { get; set; }
        [NotMapped]
        public decimal? LibraryFee { get; set; }
        [NotMapped]
        public decimal? OtherFee { get; set; }
        [NotMapped]
        public decimal TotalFee { get; set; }

        //[NotMapped]
        public List<SelectListItem> Genders { get; set; }

        //[NotMapped]
        public List<SelectListItem> AcademicYears { get; set; }

        //[NotMapped]
        public List<SelectListItem> Classes { get; set; }


        // ✅ Custom Validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateOfBirth.HasValue)
            {
                var today = DateTime.Today;

                if (DateOfBirth > today)
                    yield return new ValidationResult("DOB cannot be in future", new[] { "DateOfBirth" });

                var age = today.Year - DateOfBirth.Value.Year;
                if (DateOfBirth.Value > today.AddYears(-age)) age--;

                if (age < 5)
                    yield return new ValidationResult("Student must be at least 5 years old", new[] { "DateOfBirth" });

                if (age > 100)
                    yield return new ValidationResult("Invalid DOB", new[] { "DateOfBirth" });
            }

            if (AdmissionYear.HasValue && AdmissionYear > DateTime.Today.Year)
            {
                yield return new ValidationResult("Invalid admission year", new[] { "AdmissionYear" });
            }
        }
    }



    //public class StudentFormViewModel : IValidatableObject
    //{
    //    // -------------------- Student Info --------------------
    //    [Required(ErrorMessage = "Student Name is required.")]
    //    [StringLength(100)]
    //    public string StudentName { get; set; }

    //    [Required(ErrorMessage = "Select Gender")]
    //    public int? GenderID { get; set; }

    //    [Required(ErrorMessage = "Date of Birth is required.")]
    //    [DataType(DataType.Date)]
    //    public DateTime? DateOfBirth { get; set; }

    //    [Required(ErrorMessage = "Address is missing")]
    //    [StringLength(250)]
    //    public string PermanentAddress { get; set; }

    //    [Required(ErrorMessage = "Phone number is missing")]
    //    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone must be 10 digits")]
    //    public string PhoneNo { get; set; }

    //    [EmailAddress]
    //    [StringLength(100)]
    //    public string EmailID { get; set; }

    //    [Required(ErrorMessage = "Blood group is missing")]
    //    public string BloodGroup { get; set; }

    //    [Required(ErrorMessage = "Aadhaar Number is missing")]
    //    [RegularExpression(@"^[0-9]{12}$", ErrorMessage = "Aadhaar must be 12 digits")]
    //    public string Aadhaar { get; set; }

    //    [RegularExpression(@"^[0-9]{12}$", ErrorMessage = "APAAR ID must be 12 digits")]
    //    public string? ApaarID { get; set; } // Nullable

    //    [Required(ErrorMessage = "Father Name is missing")]
    //    [StringLength(100)]
    //    public string FatherName { get; set; }

    //    [Required(ErrorMessage = "Mother Name is missing")]
    //    [StringLength(100)]
    //    public string MotherName { get; set; }

    //    [Required(ErrorMessage = "Emergency Contact is missing")]
    //    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Emergency contact must be 10 digits")]
    //    public string EmergencyContact { get; set; }

    //    [Required(ErrorMessage = "Father Aadhaar Number is missing")]
    //    [RegularExpression(@"^[0-9]{12}$")]
    //    public string FatherAadhaar { get; set; }

    //    [Required(ErrorMessage = "Mother Aadhaar Number is missing")]
    //    [RegularExpression(@"^[0-9]{12}$")]
    //    public string MotherAadhaar { get; set; }

    //    [Required(ErrorMessage = "Select Academic Year")]
    //    public int? AcademicYearID { get; set; }

    //    [Required(ErrorMessage = "Select Class")]
    //    public int? ClassID { get; set; }

    //    [Required(ErrorMessage = "Select Section")]
    //    public int? SectionID { get; set; }

    //    public int ClassSectionID { get; set; }

    //    public int? AdmissionYear { get; set; } = DateTime.Now.Year;

    //    [NotMapped]
    //    public string? RollNumber { get; set; }

    //    // -------------------- Dropdown Lists --------------------
    //    [NotMapped]
    //    public List<SelectListItem> Genders { get; set; } = new List<SelectListItem>();

    //    [NotMapped]
    //    public List<SelectListItem> Classes { get; set; } = new List<SelectListItem>();

    //    [NotMapped]
    //    public List<SelectListItem> AcademicYears { get; set; } = new List<SelectListItem>();

    //    // -------------------- Fees --------------------
    //    [NotMapped]
    //    public decimal TuitionFee { get; set; }
    //    [NotMapped]
    //    public decimal? LabFee { get; set; }
    //    [NotMapped]
    //    public decimal? LibraryFee { get; set; }
    //    [NotMapped]
    //    public decimal? OtherFee { get; set; }
    //    [NotMapped]
    //    public decimal TotalFee { get; set; }

    //    // -------------------- Custom Validation --------------------
    //    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    //    {
    //        if (DateOfBirth.HasValue)
    //        {
    //            var today = DateTime.Today;
    //            if (DateOfBirth > today)
    //                yield return new ValidationResult("DOB cannot be in future", new[] { nameof(DateOfBirth) });

    //            var age = today.Year - DateOfBirth.Value.Year;
    //            if (DateOfBirth.Value > today.AddYears(-age)) age--;
    //            if (age < 5) yield return new ValidationResult("Student must be at least 5 years old", new[] { nameof(DateOfBirth) });
    //            if (age > 100) yield return new ValidationResult("Invalid DOB", new[] { nameof(DateOfBirth) });
    //        }

    //        if (AdmissionYear.HasValue && AdmissionYear > DateTime.Today.Year)
    //            yield return new ValidationResult("Invalid admission year", new[] { nameof(AdmissionYear) });
    //    }
    //}



}
