using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;
using System.Data;

namespace SchoolERP.Services.Implementations
{
    public class StudentProfileService : IStudentProfileService
    {

        private readonly AppDbContext _context;
        private readonly ILogger<StudentProfileService> _logger;
        private readonly string _connectionString;

        public StudentProfileService(AppDbContext context,
            ILogger<StudentProfileService> logger, IConfiguration configuration
            )
        {
            _context = context;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private string HashPassword(string password)
        {
            var hasher = new PasswordHasher<object>();
            return hasher.HashPassword(null, password);
        }

        private async Task<int> GetNextRollNumberAsync(int classId, int sectionId, int academicYearId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("GenerateRollNumber", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@ClassId", classId));
                    cmd.Parameters.Add(new SqlParameter("@SectionId", sectionId));
                    cmd.Parameters.Add(new SqlParameter("@AcademicYearId", academicYearId));

                    await connection.OpenAsync();

                    var result = await cmd.ExecuteScalarAsync();

                    return Convert.ToInt32(result);
                }
            }
        }


        public async Task<CreateStudentResultDto> SaveStudentDetail(StudentFormViewModel student)
        {
            string schoolAbbr = "DAV";
            string passwordHash = HashPassword("Password@123"); // default password
            student.ApaarID = student.ApaarID == null ? "" : student.ApaarID;

            if (student.PaymentOption.ToLower() == "full")
                student.Discount = 10;
            else
                student.Discount = 0;

            if (student.RemainingAmount == null)
            {
                student.RemainingAmount = 0;
                student.PaidMonths = student.Months;
            }

            try
            {
                var result = await _context
                    .Set<CreateStudentResultDto>()
                    .FromSqlInterpolated($@"EXEC sp_CreateStudentWithUser 
                        @SchoolAbbr = {schoolAbbr},
                        @PasswordHash = {passwordHash},

                        @StudentName = {student.StudentName},
                        @GenderID = {student.GenderID},
                        @Aadhaar = {student.Aadhaar},
                        @PhoneNo = {student.PhoneNo},
                        @EmailID = {student.EmailID},
                        @DateOfBirth = {student.DateOfBirth},
                        @BloodGroup = {student.BloodGroup},
                        @ApaarID = {student.ApaarID},
                        @MedicalCondition = {student.MedicalCondition},

                        @PreviousSchoolName = {student.PreviousSchoolName}, 
                        @LastClassStudied = {student.LastClassStudied}, 
                        @LastClassResult = {student.LastClassResult}, 
                        @MediumOfEducation = {student.MediumOfEducation},

                        @AdmissionYear = {student.AdmissionYear},
                        @ClassSectionId = {student.ClassSectionID},
                        @AcademicYearID = {student.AcademicYearID},

                        @FatherName = {student.FatherName},
                        @FatherAadhaar = {student.FatherAadhaar},
                        @MotherName = {student.MotherName},
                        @MotherAadhaar = {student.MotherAadhaar},
                        @EmergencyContact = {student.EmergencyContact},
                        @PermanentAddress = {student.PermanentAddress},
                        @State  = {student.State},
                        @City = {student.City},
                        @Pincode  = {student.Pincode},
                        @Category  = {student.Category},
                        @Religion  = {student.Religion},
                        
                        @ParentOccupation  = {student.ParentOccupation},
                        @ParentEmail = {student.ParentEmail},
                        @AnnualIncome = {student.AnnualIncome},

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

                var data = result.FirstOrDefault();

                //if(data != null)
                //{
                //    // Step 2: Generate Roll No
                //    int rollNo = await GetNextRollNumberAsync(student.ClassID.Value
                //        , student.SectionID.Value, student.AcademicYearID.Value);

                //    InsertEnrollmentAsync(result.StudentID, student.ClassID.Value,
                //        student.SectionID.Value, student.AcademicYearID.Value, rollNo);
                //}

                return data; // ✅ return SP result directly
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

        private async Task InsertEnrollmentAsync(
            int studentId,
            int classId,
            int sectionId,
            int academicYearId,
            int rollNo)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"
                INSERT INTO StudentEnrollments
                (StudentId, ClassId, SectionId, AcademicYearId, RollNumber, Status, IsActive, CreatedDate)
                VALUES
                (@StudentId, @ClassId, @SectionId, @AcademicYearId, @RollNumber, 'Active', 1, GETDATE());
            ";
            using var cmd = new SqlCommand(query, connection);

            cmd.Parameters.AddWithValue("@StudentId", studentId);
            cmd.Parameters.AddWithValue("@ClassId", classId);
            cmd.Parameters.AddWithValue("@SectionId", sectionId);
            cmd.Parameters.AddWithValue("@AcademicYearId", academicYearId);
            cmd.Parameters.AddWithValue("@RollNumber", rollNo);

            await cmd.ExecuteNonQueryAsync();
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

                               // Enrollment mapping
                           join se in _context.StudentEnrollments
                                on s.StudentID equals se.StudentId

                           join c in _context.Classes
                               on se.ClassId equals c.ClassId

                           join sec in _context.Sections
                               on se.SectionId equals sec.SectionId

                           // Parent link
                           join ps in _context.ParentStudent
                                on s.StudentID equals ps.StudentId into psGroup
                           from ps in psGroup.DefaultIfEmpty()

                               // Parent details
                           join p in _context.Parents
                                on ps.ParentId equals p.ParentId into parentGroup
                           from p in parentGroup.DefaultIfEmpty()
                           where s.UserID == id && se.Status == "Active"
                           select new EditStudentProfileDto
                           {
                               Id = s.StudentID,

                               // Personal
                               StudentName = s.StudentName,
                               GenderID = s.GenderID,
                               DateOfBirth = s.DateOfBirth,
                               BloodGroup = s.BloodGroup,
                               Category = p.Category,
                               Religion = p.Religion,

                               // Read-only
                               AdmissionNumber = s.AdmissionNumber,
                               RollNumber = s.RollNumber,
                               AdmissionYear = s.AdmissionYear ?? 0,

                               // Contact
                               PhoneNo = s.PhoneNo,
                               EmailID = s.EmailID,
                               PermanentAddress = p.Address,
                               City = p.City,
                               State = p.State,
                               Pincode = p.Pincode,
                               EmergencyContact = p.EmergencyContact,

                               // Parent Details
                               FatherName = p != null ? p.FatherName : "",
                               MotherName = p != null ? p.MotherName : "",
                               ParentOccupation = p != null ? p.ParentOccupation : "",
                               ParentEmail = p != null ? p.ParentEmail : "",
                               AnnualIncome = p != null ? p.AnnualIncome : null,

                               FatherAadhaar = p != null ? MaskAadhaar(p.FatherAadhaar) : "",
                               MotherAadhaar = p != null ? MaskAadhaar(p.MotherAadhaar) : "",

                               // Identity
                               AadhaarMasked = MaskAadhaar(s.Aadhaar),
                               ApaarIDMasked = MaskAadhaar(s.ApaarID),

                               // Academic
                               Class = c.ClassName,
                               Section = sec.SectionName,
                               AcademicYear = se.AcademicYear != null ? se.AcademicYear.YearName : "",

                               LastClassStudied = s.LastClassStudied,
                               LastClassResult = s.LastClassResult,
                               PreviousSchoolName = s.PreviousSchoolName,
                               MediumOfEducation = s.MediumOfEducation,

                               ProfileImagePath = s.ProfileImagePath
                           })
                   .FirstOrDefaultAsync();



                //var student =
                //    await (from s in _context.Students
                //    join cs in _context.ClassSections on s.ClassSectionId equals cs.Id
                //    join c in _context.Classes on cs.ClassId equals c.ClassId
                //    join sec in _context.Sections on cs.SectionId equals sec.SectionId
                //    where s.UserID == id
                //    select new EditStudentProfileDto
                //    {
                //        Id = s.StudentID,
                //        // Personal
                //        StudentName = s.StudentName,
                //        GenderID = s.GenderID,
                //        DateOfBirth = s.DateOfBirth,
                //        BloodGroup = s.BloodGroup,
                //        Category = s.Category,
                //        Religion = s.Religion,

                //        // Read-only
                //        AdmissionNumber = s.AdmissionNumber,
                //        RollNumber = s.RollNumber,
                //        AdmissionYear = s.AdmissionYear ?? 0,

                //        // Contact
                //        PhoneNo = s.PhoneNo,
                //        EmailID = s.EmailID,
                //        PermanentAddress = s.PermanentAddress,
                //        City = s.City,
                //        State = s.State,
                //        Pincode = s.Pincode,
                //        EmergencyContact = s.EmergencyContact,

                //        // Family
                //        FatherName = s.FatherName,
                //        MotherName = s.MotherName,
                //        ParentOccupation = s.ParentOccupation,
                //        ParentEmail = s.ParentEmail,
                //        AnnualIncome = s.AnnualIncome,

                //        FatherAadhaar = MaskAadhaar(s.FatherAadhaar),
                //        MotherAadhaar = MaskAadhaar(s.MotherAadhaar),

                //        // Identity (masked)
                //        AadhaarMasked = MaskAadhaar(s.Aadhaar),
                //        ApaarIDMasked = MaskAadhaar(s.ApaarID),

                //        // Medical
                //        //MedicalCondition = s.MedicalCondition,

                //        // Academic
                //        Class = c.ClassName,
                //        Section = sec.SectionName,
                //        AcademicYear = s.AcademicYear != null ? s.AcademicYear.YearName : "",
                //        LastClassStudied = s.LastClassStudied,
                //        LastClassResult = s.LastClassResult,
                //        PreviousSchoolName = s.PreviousSchoolName,
                //        MediumOfEducation = s.MediumOfEducation,
                //        ProfileImagePath = s.ProfileImagePath
                //    })
                //    .FirstOrDefaultAsync();

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
                join u in _context.Users on s.UserID equals u.UserID
                join se in _context.StudentEnrollments on s.StudentID equals se.StudentId
                join c in _context.Classes on se.ClassId equals c.ClassId
                join sec in _context.Sections on se.SectionId equals sec.SectionId
                join stup in _context.ParentStudent on s.StudentID equals stup.StudentId
                join p in _context.Parents on stup.ParentId equals p.ParentId
                where s.UserID == id && se.Status == "Active"

                select new StudentProfileDto
                {
                    Name = s.StudentName,
                    RollNumber = s.RollNumber,

                    Email = s.EmailID,
                    Gender = s.GenderID,
                    Phone = s.PhoneNo,

                    Address = p.Address,

                    Class = c.ClassName,
                    Section = sec.SectionName,

                    Year = s.AdmissionYear ?? 0,

                    UserName = u.UserName,
                    LastLoginAt = u.LastLoginAt,
                    PreviousLoginAt = u.PreviousLoginAt,
                    IsActive = u.IsActive,

                    ProfileImagePath = s.ProfileImagePath
                }
             ).FirstOrDefaultAsync();


            //var dto = await (
            //    from s in _context.Students
            //    join u in _context.Users
            //        on s.UserID equals u.UserID
            //    join cs in _context.ClassSections on s.ClassSectionId equals cs.Id
            //    join c in _context.Classes on cs.ClassId equals c.ClassId
            //    join sec in _context.Sections on cs.SectionId equals sec.SectionId
            //    where s.UserID == id
            //    select new StudentProfileDto
            //    {
            //        Name = s.StudentName,
            //        RollNumber = s.RollNumber,
            //        Email = s.EmailID,
            //        Gender = s.GenderID,
            //        Phone = s.PhoneNo,
            //        Address = s.PermanentAddress,

            //        Class = c.ClassName,
            //        Section = sec.SectionName,
            //        Year = s.AdmissionYear.Value, 

            //        UserName = u.UserName,
            //        LastLoginAt = u.LastLoginAt,
            //        PreviousLoginAt = u.PreviousLoginAt,
            //        IsActive = u.IsActive,
            //        ProfileImagePath = Convert.ToString(s.ProfileImagePath)
            //    }
            //).FirstOrDefaultAsync();

            return dto ?? new StudentProfileDto(); // Return empty Student if not found
        }

        public async Task UpdateStudentProfileAsync(EditStudentProfileDto model)
        {
            try
            {
                var student = await _context.Students.FindAsync(model.Id);

                if (student == null)
                    throw new Exception("Student not found");

                // =========================
                // Update Student Table
                // =========================
                student.StudentName = model.StudentName;
                student.GenderID = model.GenderID;
                student.DateOfBirth = model.DateOfBirth;
                student.BloodGroup = model.BloodGroup;

                student.PhoneNo = model.PhoneNo;
                student.EmailID = model.EmailID;

                // =========================
                // Get Parent Mapping
                // =========================
                var parentLink = await _context.ParentStudent
                    .FirstOrDefaultAsync(x => x.StudentId == student.StudentID);

                if (parentLink != null)
                {
                    var parent = await _context.Parents
                        .FirstOrDefaultAsync(x => x.ParentId == parentLink.ParentId);

                    if (parent != null)
                    {
                        // =========================
                        // Update Parent Table
                        // =========================
                        parent.Address = model.PermanentAddress;
                        parent.City = model.City;
                        parent.State = model.State;
                        parent.Pincode = model.Pincode;
                        parent.EmergencyContact = model.EmergencyContact;
                        parent.Category = model.Category;
                        parent.Religion = model.Religion;
                        parent.FatherName = model.FatherName;
                        parent.MotherName = model.MotherName;
                        parent.ParentOccupation = model.ParentOccupation;
                        parent.ParentEmail = model.ParentEmail;
                        parent.EmergencyContact = model.EmergencyContact;
                        parent.AnnualIncome = model.AnnualIncome;
                    }
                }

                //// 🔥 Update only editable fields
                //student.StudentName = model.StudentName;
                //student.GenderID = model.GenderID;
                //student.DateOfBirth = model.DateOfBirth;
                //student.BloodGroup = model.BloodGroup;
                //student.Category = model.Category;
                //student.Religion = model.Religion;

                //student.PhoneNo = model.PhoneNo;
                //student.EmailID = model.EmailID;
                //student.PermanentAddress = model.PermanentAddress;
                //student.City = model.City;
                //student.State = model.State;
                //student.Pincode = model.Pincode;
                //student.EmergencyContact = model.EmergencyContact;

                //student.FatherName = model.FatherName;
                //student.MotherName = model.MotherName;
                //student.ParentOccupation = model.ParentOccupation;
                //student.ParentEmail = model.ParentEmail;
                //student.AnnualIncome = model.AnnualIncome;

                // ❌ Do NOT update read-only fields:
                // AdmissionNumber, RollNumber, AdmissionYear, etc.

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating student detail in profile view!!");
            }
        }

        public async Task<StudentFormViewModel> GetStudentForEditAsync(int studentId)
        {
            try
            {

                #region Refactored Version (Clean + Safe + Faster)

                //var studentData = await _context.Students
                //    .AsNoTracking()
                //    .Where(s => s.StudentID == studentId)
                //    .Select(s => new
                //    {
                //        s,
                //        ClassSection = _context.ClassSections
                //            .Where(cs => cs.Id == s.ClassSectionId)
                //            .Select(cs => new
                //            {
                //                cs.ClassId,
                //                cs.SectionId,
                //                ClassName = _context.Classes
                //                    .Where(c => c.ClassId == cs.ClassId)
                //                    .Select(c => c.ClassName)
                //                    .FirstOrDefault(),

                //                SectionName = _context.Sections
                //                    .Where(sec => sec.SectionId == cs.SectionId)
                //                    .Select(sec => sec.SectionName)
                //                    .FirstOrDefault()
                //            })
                //            .FirstOrDefault(),

                //        AcademicYear = _context.AcademicYears
                //            .Where(ay => ay.AcademicYearID == s.AcademicYearID)
                //            .Select(ay => ay.AcademicYearID)
                //            .FirstOrDefault()
                //    })
                //    .FirstOrDefaultAsync();


                ////Fetch Fees separately (important)
                //var feeData = await _context.StudentFees
                //    .AsNoTracking()
                //    .Where(sf => sf.StudentId == studentId)
                //    .Select(sf => new
                //    {
                //        sf.TotalFee,
                //        sf.RemainingAmount,
                //        sf.DiscountApplied,
                //        sf.PaymentOption,
                //        sf.PaidMonths,
                //        CourseFee = _context.CourseFees
                //            .Where(cf => cf.CourseFeeId == sf.CourseFeeId)
                //            .Select(cf => new
                //            {
                //                cf.TuitionFee,
                //                cf.AdmissionFee,
                //                cf.TransportFee,
                //                cf.LibraryFee,
                //                cf.OtherFee
                //            })
                //            .FirstOrDefault()
                //    })
                //    .FirstOrDefaultAsync();

                ////Final Mapping To Model Call 
                //var model = new StudentFormViewModel
                //{
                //    StudentId = studentData.s.StudentID,

                //    // BASIC
                //    StudentName = studentData.s.StudentName,
                //    GenderID = studentData.s.GenderID,
                //    DateOfBirth = studentData.s.DateOfBirth,
                //    PhoneNo = studentData.s.PhoneNo,
                //    EmailID = studentData.s.EmailID,
                //    BloodGroup = studentData.s.BloodGroup,
                //    Aadhaar = studentData.s.Aadhaar,

                //    Category = studentData.s.Category,
                //    Religion = studentData.s.Religion,

                //    // ADDRESS
                //    PermanentAddress = studentData.s.PermanentAddress,
                //    City = studentData.s.City,
                //    State = studentData.s.State,
                //    Pincode = studentData.s.Pincode,

                //    // PARENTS
                //    FatherName = studentData.s.FatherName,
                //    FatherAadhaar = studentData.s.FatherAadhaar,
                //    MotherName = studentData.s.MotherName,
                //    MotherAadhaar = studentData.s.MotherAadhaar,
                //    EmergencyContact = studentData.s.EmergencyContact,
                //    ParentEmail = studentData.s.ParentEmail,

                //    // CLASS
                //    ClassID = studentData.ClassSection?.ClassId ?? 0,
                //    SectionID = studentData.ClassSection?.SectionId ?? 0,
                //    ClassName = studentData.ClassSection?.ClassName,
                //    SectionName = studentData.ClassSection?.SectionName,

                //    // YEAR
                //    AcademicYearID = studentData.AcademicYear,
                //    AdmissionYear = studentData.s.AdmissionYear,
                //    AdmissionNumber = studentData.s.AdmissionNumber,
                //    RollNumber = studentData.s.RollNumber,

                //    // MEDICAL
                //    MedicalCondition = studentData.s.MedicalCondition,
                //    IsActive = studentData.s.IsActive,

                //    // FEES
                //    TuitionFee = feeData?.CourseFee?.TuitionFee ?? 0,
                //    AdmissionFee = feeData?.CourseFee?.AdmissionFee ?? 0,
                //    TransportFee = feeData?.CourseFee?.TransportFee ?? 0,
                //    LibraryFee = feeData?.CourseFee?.LibraryFee ?? 0,
                //    OtherFee = feeData?.CourseFee?.OtherFee ?? 0,

                //    TotalFee = feeData?.TotalFee ?? 0,
                //    Discount = feeData?.DiscountApplied ?? 0,
                //    FinalAmount = (feeData?.TotalFee ?? 0) - (feeData?.DiscountApplied ?? 0),
                //    RemainingAmount = feeData?.RemainingAmount ?? 0,

                //    PaymentOption = feeData?.PaymentOption,
                //    PaidMonths = feeData?.PaidMonths
                //};

                #endregion

                var model = await (
                    from s in _context.Students
                    join se in _context.StudentEnrollments on s.StudentID equals se.StudentId
                    
                    //// ================= CLASS + SECTION =================
                    //join cs in _context.ClassSections on s.ClassSectionId equals cs.Id

                    join c in _context.Classes on se.ClassId equals c.ClassId
                    join sec in _context.Sections on se.SectionId equals sec.SectionId

                    join ps in _context.ParentStudent on s.StudentID equals ps.StudentId into psGroup
                    from ps in psGroup.DefaultIfEmpty()

                    join p in _context.Parents on ps.ParentId equals p.ParentId into parentGroup
                    from p in parentGroup.DefaultIfEmpty()

                        // ================= ACADEMIC YEAR =================
                    join ay in _context.AcademicYears on s.AcademicYearID equals ay.AcademicYearID

                    // ================= STUDENT FEES (LEFT JOIN) =================
                    join sf in _context.StudentFees on s.StudentID equals sf.StudentId into sfGroup
                    from sf in sfGroup.DefaultIfEmpty()

                        // ================= COURSE FEES =================
                    join cf in _context.CourseFees
                        on sf.CourseFeeId equals cf.CourseFeeId into cfGroup
                    from cf in cfGroup.DefaultIfEmpty()

                    where s.StudentID == studentId

                    select new StudentFormViewModel
                    {
                        StudentId = s.StudentID,
                        // ================= BASIC =================
                        StudentName = s.StudentName,
                        GenderID = s.GenderID,
                        DateOfBirth = s.DateOfBirth,
                        PhoneNo = s.PhoneNo,
                        EmailID = s.EmailID,
                        BloodGroup = s.BloodGroup,
                        Aadhaar = s.Aadhaar,

                        Category = Convert.ToString(p.Category),
                        Religion = Convert.ToString(p.Religion),

                        PreviousSchoolName = s.PreviousSchoolName,
                        LastClassStudied = s.LastClassStudied,
                        LastClassResult = s.LastClassResult,
                        MediumOfEducation = s.MediumOfEducation,
                        // ================= ADDRESS =================
                        PermanentAddress = p.Address,
                        City = p.City,
                        State = p.State,
                        Pincode = p.Pincode,

                        // ================= PARENTS =================
                        FatherName = p.FatherName,
                        FatherAadhaar = p.FatherAadhaar,
                        MotherName = p.MotherName,
                        MotherAadhaar = p.MotherAadhaar,
                        EmergencyContact = p.EmergencyContact,
                        ParentEmail = p.ParentEmail,
                        ParentOccupation = p.ParentOccupation,
                        AnnualIncome = p.AnnualIncome,

                        // ================= CLASS =================
                        ClassID = c.ClassId,
                        SectionID = sec.SectionId,
                        ClassName = c.ClassName,
                        SectionName = sec.SectionName,

                        // ================= YEAR =================
                        AcademicYearID = ay.AcademicYearID,
                        AdmissionYear = s.AdmissionYear,
                        AdmissionNumber = Convert.ToString(s.AdmissionNumber),
                        RollNumber = Convert.ToString(s.RollNumber),
                        // ================= MEDICAL =================
                        MedicalCondition = Convert.ToString(s.MedicalCondition),
                        IsActive = s.IsActive,

                        // ================= COURSE FEES =================
                        TuitionFee = cf != null ? cf.TuitionFee : 0,
                        
                        AdmissionFee = cf != null ? cf.AdmissionFee : 0,
                        TransportFee = cf != null ? cf.TransportFee : 0,
                        LibraryFee = cf != null ? cf.LibraryFee : 0,
                        OtherFee = cf != null ? cf.OtherFee : 0,
                        TotalFee = sf != null ? sf.TotalFee : 0,

                        // ================= STUDENT FEES =================
                        FinalAmount = sf != null ? (sf.TotalFee - (sf.DiscountApplied ?? 0)) : 0,
                        Discount = sf != null ? (sf.DiscountApplied ?? 0) : 0,
                        RemainingAmount = sf != null ? sf.RemainingAmount : 0,

                        // ===== Student Fee Info =====
                        PaymentOption = sf != null ? sf.PaymentOption : null,
                        PaidMonths = sf.PaidMonths,
                        //Status = sf.Status
                    }
                    
                    ).FirstOrDefaultAsync();


                // Load dropdowns
                model.Genders = await GetGendersAsync();
                model.Classes = await GetClassesAsync();
                model.AcademicYears = await GetAcademicYearsAsync();

                return model;
            }
            catch (SqlException ex) when (ex.Number == -2) // timeout
            {
                // log it
                _logger.LogError(ex, "Database timeout");

                // return safe response
                throw new Exception("The request took too long. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while reading student detail!!");
                return (new StudentFormViewModel());
            }
        }

        public async Task<StudentFormViewModel> RebuildEditModelAsync(StudentFormViewModel model)
        {
            model.Genders = await GetGendersAsync();
            model.Classes = await GetClassesAsync();
            model.AcademicYears = await GetAcademicYearsAsync();

            return model;
        }

        public async Task<bool> UpdateStudentAsync(StudentFormViewModel model)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(x => x.StudentID == model.StudentId);

            if (student == null)
                return false;

            // ================= BASIC =================
            student.StudentName = model.StudentName;
            student.GenderID = model.GenderID;
            student.DateOfBirth = model.DateOfBirth;
            student.PhoneNo = model.PhoneNo;
            student.EmailID = model.EmailID;
            student.BloodGroup = model.BloodGroup;
            student.ApaarID = model.ApaarID.Trim();

            // ================= MEDICAL =================
            student.MedicalCondition = model.MedicalCondition;

            // =========================
            var parentLink = await _context.ParentStudent
                .FirstOrDefaultAsync(x => x.StudentId == student.StudentID);

            if(parentLink != null)
            {
                var parent = await _context.Parents
                        .FirstOrDefaultAsync(x => x.ParentId == parentLink.ParentId);

                if (parent != null) {
                    parent.Address = model.PermanentAddress;
                    parent.City = model.City;
                    parent.State = model.State;
                    parent.Pincode = model.Pincode;
                    parent.EmergencyContact = model.EmergencyContact;
                    parent.Category = model.Category;
                    parent.Religion = model.Religion;
                    parent.FatherName = model.FatherName;
                    parent.MotherName = model.MotherName;
                    parent.ParentOccupation = model.ParentOccupation;
                    parent.ParentEmail = model.ParentEmail;
                    parent.AnnualIncome = model.AnnualIncome;
                }
            }

            //// ================= ACADEMIC =================
            //student.ClassSectionId = model.ClassSectionID;
            //student.AcademicYearID = model.AcademicYearID;

            //// ================= PARENTS =================
            //student.FatherName = model.FatherName;
            //student.MotherName = model.MotherName;
            //student.EmergencyContact = model.EmergencyContact;
            //student.ParentEmail = model.ParentEmail;

            await _context.SaveChangesAsync();

            return true;
        }
        // ================= HELPERS =================

        private async Task<List<SelectListItem>> GetGendersAsync()
        {
            return await _context.Genders
                .Select(g => new SelectListItem
                {
                    Value = g.GenderID.ToString(),
                    Text = g.GenderName
                }).ToListAsync();
        }

        private async Task<List<SelectListItem>> GetClassesAsync()
        {
            return await _context.Classes
                .OrderBy(c => c.ClassOrder)
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId.ToString(),
                    Text = c.ClassName
                }).ToListAsync();
        }

        private async Task<List<SelectListItem>> GetAcademicYearsAsync()
        {
            return await _context.AcademicYears
                .OrderByDescending(a => a.YearName)
                .Select(a => new SelectListItem
                {
                    Value = a.AcademicYearID.ToString(),
                    Text = a.YearName
                }).ToListAsync();
        }


    }
}
