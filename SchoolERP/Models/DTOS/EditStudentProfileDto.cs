using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.DTOS
{
    public class EditStudentProfileDto
    {
        public int Id { get; set; }
        // Personal
        [Required]
        public string StudentName { get; set; }

        [Required]
        public int GenderID { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string BloodGroup { get; set; }
        public string Category { get; set; }
        public string Religion { get; set; }

        // Read-only
        public string? AdmissionNumber { get; set; }
        public string? RollNumber { get; set; }
        public int? AdmissionYear { get; set; }

        // Contact
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid phone number")]
        public string PhoneNo { get; set; }

        public string EmailID { get; set; }

        public string PermanentAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        [RegularExpression(@"^\d{6}$", ErrorMessage = "Invalid Pincode")]
        public string Pincode { get; set; }

        public string EmergencyContact { get; set; }

        // Family
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string ParentOccupation { get; set; }

        [EmailAddress]
        public string ParentEmail { get; set; }

        public decimal? AnnualIncome { get; set; }

        public string FatherAadhaar { get; set; }
        public string MotherAadhaar { get; set; }

        // Identity (masked)
        public string? AadhaarMasked { get; set; }
        public string? ApaarIDMasked { get; set; }

        // Medical
        //public string? MedicalCondition { get; set; }

        // Academic (read-only)
        public string? Class {  get; set; }
        public string? Section { get; set; }
        public string AcademicYear { get; set; }
        public string? LastClassStudied { get; set; }
        public decimal? LastClassResult { get; set; }
        public string? PreviousSchoolName { get; set; }
        public string? MediumOfEducation { get; set; }

        public string? ProfileImagePath { get; set; }
    }



}
