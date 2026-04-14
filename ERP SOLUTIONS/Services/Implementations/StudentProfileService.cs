using ERP_SOLUTIONS.Data;
using ERP_SOLUTIONS.Models.DTOS;
using ERP_SOLUTIONS.Models.Entities;
using ERP_SOLUTIONS.Models.ViewModels;
using ERP_SOLUTIONS.Services.Interfaces;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ERP_SOLUTIONS.Services.Implementations
{
    public class StudentProfileService : IStudentProfileService
    {

        private readonly AppDbContext _context;
        private readonly ILogger<StudentProfileService> _logger;

        public StudentProfileService(AppDbContext context, ILogger<StudentProfileService> logger)
        {
            _context = context;
            _logger = logger;
        }
        private string HashPassword(string password)
        {
            var hasher = new PasswordHasher<object>();
            return hasher.HashPassword(null, password);
        }

        public async Task<CreateStudentResultDto> SaveStudentDetail(StudentFormViewModel student)
        {
            string schoolAbbr = "DAV";
            string passwordHash = HashPassword("Password@123"); // default password
            student.ApaarID = student.ApaarID == null ? "" : student.ApaarID;
            student.Discount = 10;

            if(student.RemainingAmount == null)
            {
                student.RemainingAmount = 0;
                student.PaidMonths = student.Months;
            }

            try
            {
                var list = await _context.CreateStudentResults
                    .FromSqlInterpolated($@"EXEC sp_CreateStudentWithUser 
                        @SchoolAbbr = {schoolAbbr},
                        @PasswordHash = {passwordHash},
                        @StudentName = {student.StudentName},
                        @GenderID = {student.GenderID},
                        @PermanentAddress = {student.PermanentAddress},
                        @Aadhaar = {student.Aadhaar},
                        @PhoneNo = {student.PhoneNo},
                        @EmailID = {student.EmailID},
                        @DateOfBirth = {student.DateOfBirth},
                        @BloodGroup = {student.BloodGroup},
                        @FatherName = {student.FatherName},
                        @FatherAadhaar = {student.FatherAadhaar},
                        @MotherName = {student.MotherName},
                        @MotherAadhaar = {student.MotherAadhaar},
                        @EmergencyContact = {student.EmergencyContact},
                        @ApaarID = {student.ApaarID},
                        @AdmissionYear = {student.AdmissionYear},
                        @ClassSectionId = {student.ClassSectionID},
                        @AcademicYearID = {student.AcademicYearID},

                        @State  = {student.State},
                        @City = {student.City},
                        @Pincode  = {student.Pincode},
                        @Category  = {student.Category},
                        @Religion  = {student.Religion},
                        @MedicalCondition = {student.MedicalCondition},
                        @ParentOccupation  = {student.ParentOccupation},
                        @ParentEmail = {student.ParentEmail},
                        @AnnualIncome = {student.AnnualIncome},

                        @PreviousSchoolName = {student.PreviousSchoolName}, 
                        @LastClassStudied = {student.LastClassStudied}, 
                        @LastClassResult = {student.LastClassResult}, 
                        @MediumOfEducation = {student.MediumOfEducation},

                        @CourseFeeId = {student.CourseFeeId}, 
	                    @PaymentOption = {student.PaymentOption},
	                    @DiscountApplied = {student.Discount}, 
	                    @TotalFee = {student.FinalAmount}, 
	                    @IncludeTransport = {student.IncludeTransport}, 
	                    @Months = {student.Months}, 
	                    @PaidMonths = {student.PaidMonths}, 
	                    @MonthlyAmount = {student.MonthlyAmount},
	                    @RemainingAmount = {student.RemainingAmount}

                    ").ToListAsync();

                var result = list.FirstOrDefault();

                if(result != null)
                {
                    
                }

                return result; // ✅ return SP result directly
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding new student!!");

                // 🔴 This will catch RAISERROR from SQL
                string error = ex.Message;
                // ✅ handle SQL errors (RAISERROR case)
                return new CreateStudentResultDto
                {
                    Status = 0,
                    Message = ex.Message,
                    UserName = "",
                    UserID = 0,
                    StudentID = 0,
                    RollNumber = ""
                };
                // log or return
            }
        }

        private static string MaskAadhaar(string aadhaar)
        {
            if (string.IsNullOrEmpty(aadhaar) || aadhaar.Length < 4)
                return "XXXX-XXXX-XXXX";

            return "XXXX-XXXX-" + aadhaar.Substring(aadhaar.Length - 4);
        }

        public async Task<EditStudentProfileDto> GetStudentProfileData(int id)
        {
            try
            {
                var student =
                    await (from s in _context.Students
                    join cs in _context.ClassSections on s.ClassSectionId equals cs.Id
                    join c in _context.Classes on cs.ClassId equals c.ClassId
                    join sec in _context.Sections on cs.SectionId equals sec.SectionId
                    where s.UserID == id
                    select new EditStudentProfileDto
                    {
                        Id = s.StudentID,
                        // Personal
                        StudentName = s.StudentName,
                        GenderID = s.GenderID,
                        DateOfBirth = s.DateOfBirth,
                        BloodGroup = s.BloodGroup,
                        Category = s.Category,
                        Religion = s.Religion,
                        
                        // Read-only
                        AdmissionNumber = s.AdmissionNumber,
                        RollNumber = s.RollNumber,
                        AdmissionYear = s.AdmissionYear ?? 0,

                        // Contact
                        PhoneNo = s.PhoneNo,
                        EmailID = s.EmailID,
                        PermanentAddress = s.PermanentAddress,
                        City = s.City,
                        State = s.State,
                        Pincode = s.Pincode,
                        EmergencyContact = s.EmergencyContact,

                        // Family
                        FatherName = s.FatherName,
                        MotherName = s.MotherName,
                        ParentOccupation = s.ParentOccupation,
                        ParentEmail = s.ParentEmail,
                        AnnualIncome = s.AnnualIncome,

                        FatherAadhaar = MaskAadhaar(s.FatherAadhaar),
                        MotherAadhaar = MaskAadhaar(s.MotherAadhaar),

                        // Identity (masked)
                        AadhaarMasked = MaskAadhaar(s.Aadhaar),
                        ApaarIDMasked = MaskAadhaar(s.ApaarID),

                        // Medical
                        //MedicalCondition = s.MedicalCondition,

                        // Academic
                        Class = c.ClassName,
                        Section = sec.SectionName,
                        AcademicYear = s.AcademicYear != null ? s.AcademicYear.YearName : "",
                        LastClassStudied = s.LastClassStudied,
                        LastClassResult = s.LastClassResult,
                        PreviousSchoolName = s.PreviousSchoolName,
                        MediumOfEducation = s.MediumOfEducation
                    })
                    .FirstOrDefaultAsync();
                

                return student ?? new EditStudentProfileDto(); // Return empty Student if not found
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while reading student detail for profile view!!");
                return new EditStudentProfileDto();
            }
        }


        public async Task<StudentProfileDto> GetStudentProfileAsync(int id)
        {
            var dto = await (
                from s in _context.Students
                join u in _context.Users
                    on s.UserID equals u.UserID
                join cs in _context.ClassSections on s.ClassSectionId equals cs.Id
                join c in _context.Classes on cs.ClassId equals c.ClassId
                join sec in _context.Sections on cs.SectionId equals sec.SectionId
                where s.UserID == id
                select new StudentProfileDto
                {
                    Name = s.StudentName,
                    RollNumber = s.RollNumber,
                    Email = s.EmailID,
                    Gender = s.GenderID,
                    Phone = s.PhoneNo,
                    Address = s.PermanentAddress,

                    Class = c.ClassName,
                    Section = sec.SectionName,
                    Year = s.AdmissionYear.Value, 

                    UserName = u.UserName,
                    LastLoginAt = u.LastLoginAt,
                    PreviousLoginAt = u.PreviousLoginAt,
                    IsActive = u.IsActive
                }
            ).FirstOrDefaultAsync();

            return dto ?? new StudentProfileDto(); // Return empty Student if not found
        }

        public async Task UpdateStudentProfileAsync(EditStudentProfileDto model)
        {
            try
            {
                var student = await _context.Students.FindAsync(model.Id);
                if (student == null) throw new Exception("Student not found");

                // 🔥 Update only editable fields
                student.StudentName = model.StudentName;
                student.GenderID = model.GenderID;
                student.DateOfBirth = model.DateOfBirth;
                student.BloodGroup = model.BloodGroup;
                student.Category = model.Category;
                student.Religion = model.Religion;

                student.PhoneNo = model.PhoneNo;
                student.EmailID = model.EmailID;
                student.PermanentAddress = model.PermanentAddress;
                student.City = model.City;
                student.State = model.State;
                student.Pincode = model.Pincode;
                student.EmergencyContact = model.EmergencyContact;

                student.FatherName = model.FatherName;
                student.MotherName = model.MotherName;
                student.ParentOccupation = model.ParentOccupation;
                student.ParentEmail = model.ParentEmail;
                student.AnnualIncome = model.AnnualIncome;

                // ❌ Do NOT update read-only fields:
                // AdmissionNumber, RollNumber, AdmissionYear, etc.

                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error while updating student detail in profile view!!");
            }
        }
    }
}
