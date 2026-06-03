using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SchoolERP.Common;
using SchoolERP.Data;
using SchoolERP.Enum;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;


namespace SchoolERP.Services.Implementations
{
    public class TeacherService : ITeacherService
    {

        private readonly AppDbContext _context;
        private readonly ILogger<TeacherService> _logger;
        private readonly IDataProtector _protector;

        public TeacherService(AppDbContext context, ILogger<TeacherService> logger, IDataProtectionProvider protector)
        {
            _context = context;
            _logger = logger;
            _protector = protector.CreateProtector("TeacherIdProtector");
        }

        public async Task<IEnumerable<TeacherDto>> GetAllAsync()
        {
            // Join with Gender table to get Gender name
            var teachers = await (from t in _context.Teachers
                                  join g in _context.Genders on t.GenderID equals g.GenderID
                                  select new TeacherDto
                                  {
                                      TeacherID = t.TeacherID,
                                      TeacherName = t.FullName,
                                      Designation = t.Designation,
                                      Qualification = t.Qualification,
                                      Specialization = t.Specialization,
                                      Gender = g.GenderName,
                                      Phone = t.Phone ?? string.Empty,
                                      EmailID = t.EmailID ?? "N/A",
                                      Status = t.Status
                                  }).ToListAsync();
            return teachers;

        }

        public async Task<Teacher> GetByUserIdAsync(int userId)
        {
            try
            {
                var teacher = await _context.Teachers.AsNoTracking()
                .FirstOrDefaultAsync(t => t.UserID == userId);

                return teacher;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error while reading teacher detail !!");
                return new Teacher();
            }
        }

        public async Task<Teacher> GetByIdAsync(int teacherId)
        {
            return await _context.Teachers.FindAsync(teacherId);
        }

        public async Task<(string username, string password)> CreateTeacherWithUserAsync(Teacher teacher)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var exists = await _context.Teachers.AnyAsync(t =>
                    t.Phone == teacher.Phone &&
                    t.EmailID == teacher.EmailID &&
                    t.DateOfBirth == teacher.DateOfBirth);

                if (exists)
                {
                    return("", "Teacher already exists.");
                }

                // 1️ Generate unique username based on count
                var username = await GenerateUniqueUsernameAsync("TECH");

                // 2️ Create User
                var user = new User
                {
                    FullName = teacher.FullName,
                    MobileNo = teacher.Phone,
                    UserName = username,
                    Email = teacher.EmailID,
                    PasswordHash = HashPassword("Password@123"), // default password
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // get UserID

                // 3️⃣ Link Teacher to User
                teacher.UserID = user.UserID;
                teacher.StaffCode = await GenerateTeacherCodeAsync();
                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();

                //Get RoleId by Role Name
                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == "Teacher");

                if (role != null)
                {
                    var roleId = role.RoleID;

                    var userRole = new UserRole
                    {
                        UserID = teacher.UserID,
                        RoleID = roleId
                    };
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return (username, "Password@123");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error in CreateTeacherWithUserAsync !!");
                throw;
            }
        }

        //private async Task<string> GenerateTeacherCodeAsync()
        //{
        //    int year = DateTime.Now.Year;

        //    var connection = _context.Database.GetDbConnection();

        //    await using var command = connection.CreateCommand();
        //    command.CommandText = "SELECT NEXT VALUE FOR StaffCodeSeq";

        //    if (connection.State != System.Data.ConnectionState.Open)
        //        await connection.OpenAsync();

        //    var result = await command.ExecuteScalarAsync();
        //    int seq = Convert.ToInt32(result);

        //    return $"SCH-TEA-{year}-{seq:D4}";
        //}

        private async Task<string> GenerateTeacherCodeAsync()
        {
            int year = DateTime.Now.Year;

            var connection = _context.Database.GetDbConnection();
            await using var command = connection.CreateCommand();
            command.CommandText ="SELECT NEXT VALUE FOR StaffCodeSeq";

            // IMPORTANT
            command.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var result =
                await command.ExecuteScalarAsync();

            int seq =
                Convert.ToInt32(result);

            return $"SCH-TEA-{year}-{seq:D4}";
        }



        //private string GenerateStaffCode()
        //{
        //    string prefix = "SCH-TEA";
        //    int year = DateTime.Now.Year;

        //    int count = _context.Teachers.Count() + 1;

        //    return $"{prefix}-{year}-{count.ToString("D4")}";
        //}

        public async Task<(bool Status, string message)> UpdateAsync(Teacher teacher, string role)
        {

            var existingTeacher = await _context.Teachers
        .FirstOrDefaultAsync(t => t.TeacherID == teacher.TeacherID);

            if (existingTeacher == null)
                return (false, "Teacher not found.");


            // 🔍 validation (optimized: no duplicate query if unchanged)
            if (existingTeacher.Phone != teacher.Phone ||
                existingTeacher.EmailID != teacher.EmailID ||
                existingTeacher.DateOfBirth != teacher.DateOfBirth)
            {
                var exists = await _context.Teachers.AnyAsync(t =>
                    t.Phone == teacher.Phone &&
                    t.EmailID == teacher.EmailID &&
                    t.DateOfBirth == teacher.DateOfBirth &&
                    t.TeacherID != teacher.TeacherID);

                if (exists)
                    return (false, "Another teacher with same details exists.");
            }

            try
            {
                if (role == "Admin" || role == "SuperAdmin")
                {
                    // ✅ full update
                    existingTeacher.FullName = teacher.FullName;
                    existingTeacher.FullName = teacher.FullName;
                    existingTeacher.GenderID = teacher.GenderID;
                    existingTeacher.DateOfBirth = teacher.DateOfBirth;
                    existingTeacher.Aadhaar = teacher.Aadhaar;
                    existingTeacher.BloodGroup = teacher.BloodGroup;
                    existingTeacher.FatherName = teacher.FatherName;
                    existingTeacher.MotherName = teacher.MotherName;
                    existingTeacher.IsMarried = teacher.IsMarried;
                    existingTeacher.SpouseName = teacher.SpouseName;
                    existingTeacher.SpouseContact = teacher.SpouseContact;
                    existingTeacher.Phone = teacher.Phone;
                    existingTeacher.EmailID = teacher.EmailID;
                    existingTeacher.EmergencyContact = teacher.EmergencyContact;
                    existingTeacher.PermanentAddress = teacher.PermanentAddress;
                    existingTeacher.Qualification = teacher.Qualification;
                    existingTeacher.Specialization = teacher.Specialization;
                    existingTeacher.Experience = teacher.Experience;
                    existingTeacher.City = teacher.City;
                    existingTeacher.State = teacher.State;
                    existingTeacher.PostalCode = teacher.PostalCode;
                    existingTeacher.ProfileImageUrl = teacher.ProfileImageUrl;
                    existingTeacher.Designation = teacher.Designation;
                    existingTeacher.JoiningDate = teacher.JoiningDate;
                    existingTeacher.LastWorkingDate = teacher.LastWorkingDate;
                }
                else if (role == "Teacher")
                {
                    // ✅ limited update
                    // ❌ no salary, no sensitive fields

                    existingTeacher.FullName = teacher.FullName;
                    existingTeacher.GenderID = teacher.GenderID;
                    existingTeacher.DateOfBirth = teacher.DateOfBirth;
                    existingTeacher.Aadhaar = teacher.Aadhaar;
                    existingTeacher.BloodGroup = teacher.BloodGroup;
                    existingTeacher.FatherName = teacher.FatherName;
                    existingTeacher.MotherName = teacher.MotherName;
                    existingTeacher.IsMarried = teacher.IsMarried;
                    existingTeacher.SpouseName = teacher.SpouseName;
                    existingTeacher.SpouseContact = teacher.SpouseContact;
                    existingTeacher.Phone = teacher.Phone;
                    existingTeacher.EmailID = teacher.EmailID;
                    existingTeacher.EmergencyContact = teacher.EmergencyContact;
                    existingTeacher.PermanentAddress = teacher.PermanentAddress;
                    existingTeacher.City = teacher.City;
                    existingTeacher.State = teacher.State;
                    existingTeacher.PostalCode = teacher.PostalCode;
                    existingTeacher.ProfileImageUrl = teacher.ProfileImageUrl;
                    existingTeacher.Qualification = teacher.Qualification;
                    existingTeacher.Specialization = teacher.Specialization;
                    existingTeacher.Experience = teacher.Experience;
                }

                //_context.Teachers.Update(teacher);
                await _context.SaveChangesAsync();
                return (true, "Teacher detail updated successfully.");
            }
            catch (Exception)
            {
                return (false, "Teacher detail not able to update.");
            }
        }

        public async Task DeactivateAsync(int teacherId)
        {
            var teacher = await _context.Teachers.FindAsync(teacherId);
            if (teacher != null)
            {
                teacher.Status = TeacherStatus.Blocked;
                teacher.LastWorkingDate = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }
        
        private string HashPassword(string password)
        {
            var hasher = new PasswordHasher<object>();
            return hasher.HashPassword(null, password);
        }

        //Generate username based on current role count
        private async Task<string> GenerateUniqueUsernameAsync(string rolePrefix)
        {
            // Count how many users already exist for this role
            var count = await _context.Users
                .Where(u => u.UserName.StartsWith(rolePrefix))
                .CountAsync();

            int nextNumber = count + 1;

            // Dynamically determine number of digits
            int digits = nextNumber.ToString().Length; // auto adjust

            return $"{rolePrefix}_{nextNumber.ToString($"D{digits}")}";
        }

        // Teacher Methods
        public async Task<TeacherProfileDTO> GetProfileByUserIdAsync(int userId)
        {
            var profile = await (from t in _context.Teachers
                                 join g in _context.Genders on t.GenderID equals g.GenderID
                                 where t.UserID == userId
                                 select new TeacherProfileDTO
                                 {
                                     EncryptedId = _protector.Protect(t.TeacherID.ToString()),
                                     Status = t.Status,
                                     Designation = t.Designation,
                                     Aadhaar = t.Aadhaar ?? "N/A",
                                     JoiningDate = t.JoiningDate.HasValue? t.JoiningDate.Value.ToString("dd MMM yyyy") : "N/A",
                                     StatusRemark = t.StatusRemark ?? "N/A",
                                     StatusUpdatedOn = t.StatusUpdatedOn.HasValue ? t.StatusUpdatedOn.Value.ToString("dd MMM yyyy") : "N/A",
                                     TeacherID = t.TeacherID,
                                     FullName = t.FullName,
                                     GenderName = g.GenderName,   // direct join
                                     DateOfBirth = t.DateOfBirth,
                                     BloodGroup = t.BloodGroup,
                                     FatherName = t.FatherName,
                                     MotherName = t.MotherName,
                                     IsMarried = t.IsMarried,
                                     SpouseName = t.SpouseName,
                                     SpouseContact = t.SpouseContact,
                                     Phone = t.Phone,
                                     EmailID = t.EmailID,
                                     EmergencyContact = t.EmergencyContact,
                                     PermanentAddress = t.PermanentAddress,
                                     City = t.City,
                                     State = t.State,
                                     PostalCode = t.PostalCode,
                                     ProfileImageUrl = t.ProfileImageUrl,
                                     Qualification = t.Qualification,
                                     Specialization = t.Specialization,
                                     Experience = t.Experience
                                 }).FirstOrDefaultAsync();

            return profile;
        }

        private int CalculateProfileCompletion(TeacherProfileUpdateDTO teacher)
        {
            int totalFields = 14;
            int filled = 0;

            if (!string.IsNullOrWhiteSpace(teacher.FullName)) filled++;
            if (!string.IsNullOrWhiteSpace(teacher.Email)) filled++;
            if (!string.IsNullOrWhiteSpace(teacher.PhoneNumber)) filled++;
            if (!string.IsNullOrWhiteSpace(teacher.Aadhaar)) filled++;
            if (!string.IsNullOrWhiteSpace(teacher.EmergencyContact)) filled++;
            if (!string.IsNullOrWhiteSpace(teacher.PermanentAddress)) filled++;
            if (!string.IsNullOrWhiteSpace(teacher.ProfileImageUrl)) filled++;
            if (!string.IsNullOrWhiteSpace(teacher.BloodGroup)) filled++;
            //if (!string.IsNullOrWhiteSpace(teacher.Designation)) filled++;
            //if (!string.IsNullOrWhiteSpace(teacher.EmploymentType)) filled++;
            if (teacher.IsMarried)
            {
                if (!string.IsNullOrWhiteSpace(teacher.SpouseName)) filled++;
                if (!string.IsNullOrWhiteSpace(teacher.SpouseContact)) filled++;
            }
            if (!string.IsNullOrWhiteSpace(teacher.City)) filled++;
            if (!string.IsNullOrWhiteSpace(teacher.State)) filled++;
            if (!string.IsNullOrWhiteSpace(teacher.PostalCode)) filled++;
            if (teacher.GenderID > 0) filled++;

            return (filled * 100) / totalFields;
        }

        // 🔍 READ
        public async Task<ServiceResponse<TeacherProfileUpdateDTO>> GetProfileAsync(int teacherId)
        {
            try
            {
                var teacher = await _context.Teachers
                    .Where(t => t.TeacherID == teacherId)
                    .Select(t => new TeacherProfileUpdateDTO
                    {
                        Id = t.TeacherID,
                        FullName = t.FullName,
                        PhoneNumber = t.Phone,
                        EmergencyContact = t.EmergencyContact,
                        Email = t.EmailID,
                        Aadhaar = t.Aadhaar,
                        PermanentAddress = t.PermanentAddress,
                        City = t.City,
                        State = t.State,
                        PostalCode = t.PostalCode,
                        ProfileImageUrl = t.ProfileImageUrl,
                        GenderID = t.GenderID,
                        BloodGroup = t.BloodGroup,
                        IsMarried = t.IsMarried,
                        SpouseName = Convert.ToString(t.SpouseName),
                        SpouseContact = Convert.ToString(t.SpouseContact),
                        JoiningDate = t.JoiningDate.Value,
                        Designation = t.Designation,
                        EmploymentType = t.EmploymentType
                    })
                    .FirstOrDefaultAsync();

                if (teacher == null)
                    return ServiceResponse<TeacherProfileUpdateDTO>
                        .Fail("Teacher profile not found.");
                teacher.ProfileCompletion = CalculateProfileCompletion(teacher);
                return ServiceResponse<TeacherProfileUpdateDTO>
                    .Ok(teacher, "Profile fetched successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<TeacherProfileUpdateDTO>
                    .Fail($"Error fetching profile: {ex.Message}");
            }
        }

        // ✏️ UPDATE
        public async Task<ServiceResponse<TeacherProfileUpdateDTO>> UpdateProfileAsync(TeacherProfileUpdateDTO model, int userId)
        {
            try
            {
                var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserID == userId);

                if (teacher == null)
                    return ServiceResponse<TeacherProfileUpdateDTO>.Fail("Teacher not found.");

                // 🔒 Update only allowed fields
                teacher.FullName = model.FullName;
                teacher.Phone = model.PhoneNumber;
                teacher.Aadhaar = model.Aadhaar;
                teacher.EmergencyContact = model.EmergencyContact;
                teacher.PermanentAddress = model.PermanentAddress;
                teacher.City = model.City;
                teacher.State = model.State;
                teacher.PostalCode = model.PostalCode;
                teacher.ProfileImageUrl = model.ProfileImageUrl;
                teacher.GenderID = model.GenderID;
                teacher.BloodGroup = model.BloodGroup;
                if (model.IsMarried)
                {
                    //teacher.IsMarried = model.IsMarried;
                    teacher.SpouseName = model.SpouseName;
                    teacher.SpouseContact = model.SpouseContact;
                }
                else
                {
                    teacher.SpouseName = "N/A";
                    teacher.SpouseContact = "N/A";
                }
                // ⚠️ Email change (basic version)
                if (!string.Equals(teacher.EmailID, model.Email, StringComparison.OrdinalIgnoreCase))
                {
                    teacher.EmailID = model.Email;
                    // 👉 In production: trigger email verification
                }
                await _context.SaveChangesAsync();

                return ServiceResponse<TeacherProfileUpdateDTO>.Ok(model, "Profile updated successfully.");
            }
            catch (DbUpdateException)
            {
                return ServiceResponse<TeacherProfileUpdateDTO>
                    .Fail("Database error occurred while updating profile.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<TeacherProfileUpdateDTO>
                    .Fail($"Unexpected error: {ex.Message}");
            }
            
        }
    }
}
