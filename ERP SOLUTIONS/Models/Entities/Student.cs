using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace ERP_SOLUTIONS.Models.Entities
{
    public class Student
    {
        [Key]
        public int StudentID { get; set; }

        [Required]
        public int UserID { get; set; }

        // ---------------- Student Information ----------------

        [Required]
        [MaxLength(100)]
        public string StudentName { get; set; }

        [Required]
        public int GenderID { get; set; }

        [MaxLength(250)]
        public string PermanentAddress { get; set; }

        [Required]
        [MaxLength(12)]
        public string Aadhaar { get; set; }

        [MaxLength(10)]
        public string PhoneNo { get; set; }

        [MaxLength(150)]
        public string EmailID { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [NotMapped]
        public int Age => DateTime.Now.Year - DateOfBirth.Value.Year;

        [MaxLength(3)]
        public string BloodGroup { get; set; }

        [Required]
        [MaxLength(150)]
        public string FatherName { get; set; }
                
        [MaxLength(12)]
        public string FatherAadhaar { get; set; }

        [Required]
        [MaxLength(150)]
        public string MotherName { get; set; }

        [MaxLength(12)]
        public string MotherAadhaar { get; set; }

        [Required]
        [MaxLength(10)]
        public string EmergencyContact { get; set; }

        [MaxLength(12)]
        public string? ApaarID { get; set; }

        // ---------------- Academic Info ----------------

        [Required]
        [MaxLength(50)]
        public string? RollNumber { get; set; }
        
        public string? AdmissionNumber { get; set; }
        public int? AdmissionYear { get; set; }

        [Required]
        [NotMapped]
        public int ClassId { get; set; }

        [Required]
        [NotMapped]
        public int SectionID { get; set; }

        public int? ClassSectionId { get; set; }

        public int? AcademicYearID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
        public bool IsLocked { get; set; } = false;

        // ---------------- Navigation Properties ----------------

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("AcademicYearID")]
        public virtual AcademicYear AcademicYear { get; set; }

        //public ClassModel Classes { get; set; } = new();

        public string Category { get; set; }
        public string Religion { get; set; }
        public string MedicalCondition { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public string ParentOccupation { get; set; }
        public string ParentEmail { get; set; }
        public decimal? AnnualIncome { get; set; }

        public string? PreviousSchoolName { get; set; }
        public string? LastClassStudied { get; set; }
        public decimal? LastClassResult { get; set; }
        public string? MediumOfEducation { get; set; }
    }

}
