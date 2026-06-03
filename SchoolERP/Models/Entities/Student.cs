using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Models.Entities
{
    public class Student
    {
        //Student Detail
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

        [MaxLength(3)]
        public string BloodGroup { get; set; }

        [Required]
        [MaxLength(12)]
        public string Aadhaar { get; set; }

        [MaxLength(10)]
        public string PhoneNo { get; set; }

        [MaxLength(150)]
        public string EmailID { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? PreviousSchoolName { get; set; }
        public string? LastClassStudied { get; set; }
        public decimal? LastClassResult { get; set; }
        public string? MediumOfEducation { get; set; }
        public string MedicalCondition { get; set; }

        [MaxLength(12)]
        public string? ApaarID { get; set; }
        // ---------------- Academic Info ----------------

        [Required]
        [MaxLength(50)]
        public string? RollNumber { get; set; }

        public string? AdmissionNumber { get; set; }
        public int? AdmissionYear { get; set; }

        public int? ClassSectionId { get; set; }
        public int? AcademicYearID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public bool IsLocked { get; set; } = false;

        // Navigation
        //public virtual Parent Parent { get; set; }
        // Navigation
        public ICollection<ParentStudent>? ParentStudents { get; set; }
        public ICollection<StudentEnrollment>? StudentEnrollments { get; set; }

        // ---------------- Navigation Properties ----------------
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("AcademicYearID")]
        public virtual AcademicYear AcademicYear { get; set; }


        //******************************
        //Parent Detail
        //******************************
        //[MaxLength(250)]
        //public string PermanentAddress { get; set; }
        //public string City { get; set; }
        //public string State { get; set; }
        //public string Pincode { get; set; }
        //[Required]
        //[MaxLength(150)]
        //public string FatherName { get; set; }

        //[MaxLength(12)]
        //public string FatherAadhaar { get; set; }

        //[Required]
        //[MaxLength(150)]
        //public string MotherName { get; set; }

        //[MaxLength(12)]
        //public string MotherAadhaar { get; set; }

        //[Required]
        //[MaxLength(10)]
        //public string EmergencyContact { get; set; }
        //public string ParentEmail { get; set; }
        //public string Category { get; set; }
        //public string Religion { get; set; }
        //public string ParentOccupation { get; set; }
        //public decimal? AnnualIncome { get; set; }
    }




    //public class Student
    //{
    //    [Key]
    //    public int StudentID { get; set; }

    //    public int UserID { get; set; }

    //    // Student Info
    //    [Required]
    //    [MaxLength(100)]
    //    public string StudentName { get; set; }

    //    public int GenderID { get; set; }

    //    [MaxLength(3)]
    //    public string BloodGroup { get; set; }

    //    [MaxLength(12)]
    //    public string Aadhaar { get; set; }

    //    public string PhoneNo { get; set; }

    //    public string EmailID { get; set; }

    //    public DateTime? DateOfBirth { get; set; }

    //    public string? ProfileImagePath { get; set; }

    //    public string? PreviousSchoolName { get; set; }

    //    public string? LastClassStudied { get; set; }

    //    public decimal? LastClassResult { get; set; }

    //    public string? MediumOfEducation { get; set; }

    //    public string? MedicalCondition { get; set; }

    //    public string? ApaarID { get; set; }

    //    // Academic
    //    public string? RollNumber { get; set; }

    //    public string? AdmissionNumber { get; set; }

    //    public int? AdmissionYear { get; set; }

    //    public int? ClassSectionId { get; set; }

    //    public int? AcademicYearID { get; set; }

    //    public bool IsActive { get; set; } = true;

    //    public bool IsLocked { get; set; } = false;

    //    public DateTime CreatedAt { get; set; } = DateTime.Now;

    //    // Navigation
    //    public virtual Parent Parent { get; set; }

    //    public virtual User User { get; set; }

    //    public virtual AcademicYear AcademicYear { get; set; }
    //}




    //public class Parent
    //{
    //    [Key]
    //    public int ParentID { get; set; }

    //    public int StudentID { get; set; }

    //    // Address
    //    public string PermanentAddress { get; set; }

    //    public string City { get; set; }

    //    public string State { get; set; }

    //    public string Pincode { get; set; }

    //    // Parent Info
    //    public string FatherName { get; set; }

    //    public string FatherAadhaar { get; set; }

    //    public string MotherName { get; set; }

    //    public string MotherAadhaar { get; set; }

    //    public string EmergencyContact { get; set; }

    //    public string ParentEmail { get; set; }

    //    public string Category { get; set; }

    //    public string Religion { get; set; }

    //    public string ParentOccupation { get; set; }

    //    public decimal? AnnualIncome { get; set; }

    //    // Navigation
    //    [ForeignKey("StudentID")]
    //    public virtual Student Student { get; set; }
    //}


    //public class ParentStudent
    //{
    //    [Key]
    //    public int ParentStudentID { get; set; }

    //    public int ParentID { get; set; }

    //    public int StudentID { get; set; }

    //    // Navigation
    //    public Parent Parent { get; set; }

    //    public Student Student { get; set; }
    //}












}
