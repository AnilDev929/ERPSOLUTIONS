using ERP_SOLUTIONS.Data;
using ERP_SOLUTIONS.Models.DTOS;
using ERP_SOLUTIONS.Models.Entities;
using ERP_SOLUTIONS.Models.ViewModels;
using ERP_SOLUTIONS.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ERP_SOLUTIONS.Services.Implementations
{
    public class StudentProfileService : IStudentProfileService
    {

        private readonly AppDbContext _context;
        private readonly ILogger<RoleService> _logger;

        public StudentProfileService(AppDbContext context, ILogger<RoleService> logger)
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
                        @AcademicYearID = {student.AcademicYearID}
                    ").ToListAsync();

                var result = list.FirstOrDefault();

                //// success
                //if (result.Status == 1)
                //{
                //    // ✅ success
                //    var username = result.UserName;
                //    var userId = result.UserID;
                //    var studentId = result.StudentID;
                //}
                //else
                //{
                //    // ❌ handled error (no exception)
                //    string errorMessage = result.Message;
                //}

                return result; // ✅ return SP result directly
            }
            catch (Exception ex)
            {
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


        public async Task<Student> GetStudentProfileAsync(int id)
        {
            var student = await _context.Students
            .Where(s => s.StudentID == id && s.IsActive)
            .FirstOrDefaultAsync();

            return student ?? new Student(); // Return empty Student if not found
        }

        public async Task UpdateStudentProfileAsync(Student model)
        {
            var student = await _context.Students.FindAsync(model.StudentID);
            if (student == null) throw new Exception("Student not found");

            

            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }
    }
}
