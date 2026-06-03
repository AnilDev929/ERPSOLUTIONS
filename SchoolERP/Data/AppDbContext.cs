using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace SchoolERP.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<DatabaseBackup> DatabaseBackups { get; set; }
        public DbSet<MenuSection> MenuSections { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<ClassModel> Classes { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<ClassSection> ClassSections { get; set; }
        public DbSet<ClassSectionSubject> ClassSectionSubject { get; set; }
        public DbSet<CourseFee> CourseFees { get; set; }
        public DbSet<FeeConfiguration> FeeConfigurations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<AcademicCalendarExceptions> AcademicCalendarExceptions { get; set; }
        public DbSet<Hostel> Hostels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<StudentHostelAllocation> StudentHostelAllocations { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<ParentStudent> ParentStudent { get; set; }
        public DbSet<StudentFee> StudentFees { get; set; }
        public DbSet<Attendance> Attendance { get; set; }
        public DbSet<FeePayment> FeePayments { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<RoleMenuAccess> RoleMenuAccess { get; set; }

        public DbSet<Homework> Homework { get; set; }
        public DbSet<HomeworkAttachment> HomeworkAttachment { get; set; }
        public DbSet<HomeworkSubmission> HomeworkSubmission { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }


        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamTimeSlot> ExamTimeSlots { get; set; }
        public DbSet<GradeRule> GradeRules { get; set; }
        public DbSet<ExamSchedule> ExamSchedules { get; set; }
        public DbSet<StudentMarks> StudentMarks { get; set; }
        public DbSet<StudentEnrollment> StudentEnrollments { get; set; }
        public DbSet<StudentPromotionLog> StudentPromotionLogs { get; set; }
        public DbSet<PromotionSetting> PromotionSettings { get; set; }


        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationTarget> NotificationTargets { get; set; }  
        public DbSet<NotificationReadStatus> NotificationReadStatus { get; set; }

        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<CreateStudentResultDto> CreateStudentResults { get; set; }
        public DbSet<MenuSetting> MenuSettings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StudentFeeDto>().HasNoKey();

            modelBuilder.Entity<CreateStudentResultDto>().HasNoKey();

            modelBuilder.Entity<DatabaseBackup>().HasKey(x => x.BackupId);

            //FeeConfiguration
            modelBuilder.Entity<FeeConfiguration>()
               .HasKey(x => x.ConfigId);

            // 🔹 AcademicYear Config
            modelBuilder.Entity<AcademicYear>()
                .HasKey(x => x.AcademicYearID);

            modelBuilder.Entity<ExamSchedule>()
                .HasKey(x => x.ScheduleId);

            // 🔹 AcademicCalendarExceptions Config
            modelBuilder.Entity<AcademicCalendarExceptions>()
                .HasKey(x => x.ExceptionId);

            modelBuilder.Entity<Parent>(entity =>
            {
                entity.HasKey(e => e.ParentId);

                entity.HasIndex(e => e.ParentEmail).IsUnique();
                entity.HasIndex(e => e.Phone).IsUnique();

                entity.Property(e => e.AnnualIncome)
                      .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<ParentStudent>()
                .HasKey(ps => new { ps.ParentId, ps.StudentId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserID);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleID);


            modelBuilder.Entity<ClassModel>()
            .ToTable("Classes")
            .HasKey(c => c.ClassId);

            modelBuilder.Entity<ClassModel>()
                .Property(c => c.ClassId)
                .HasColumnName("ClassId");

            modelBuilder.Entity<ClassSection>()
                .Property(cs => cs.ClassId)
                .HasColumnName("ClassId");

            //modelBuilder.Entity<CourseFee>()
            //    .Property(c => c.TotalFee)
            //    .HasComputedColumnSql(
            //        "[TuitionFee] + ISNULL([AdmissionFee],0) + ISNULL([TransportFee],0) + ISNULL([LibraryFee],0) + ISNULL([OtherFee],0)"
            //    );

            modelBuilder.Entity<FeePayment>()
                    .HasKey(fp => fp.FeeId);

            modelBuilder.Entity<FeePayment>()
            .HasOne(fp => fp.StudentFee)
            .WithMany()
            .HasForeignKey(fp => fp.FeeId);

            modelBuilder.Entity<Attendance>()
            .Property(a => a.Status)
            .HasConversion<string>();

            modelBuilder.Entity<ClassSectionSubject>()
            .HasOne(x => x.ClassSection)
            .WithMany(x => x.ClassSectionSubjects)
            .HasForeignKey(x => x.ClassSectionId);

            modelBuilder.Entity<ClassSectionSubject>()
                .HasOne(x => x.Subject)
                .WithMany(x => x.ClassSectionSubjects)
                .HasForeignKey(x => x.SubjectId);

            //modelBuilder.Entity<ClassSectionSubject>()
            //    .HasOne(x => x.Teacher)
            //    .WithMany(x => x.ClassSectionSubject)
            //    .HasForeignKey(x => x.TeacherId);

            // 🔥 Prevent duplicate subject per section
            modelBuilder.Entity<ClassSectionSubject>()
                .HasIndex(x => new { x.ClassSectionId, x.SubjectId })
                .IsUnique();

            modelBuilder.Entity<NotificationReadStatus>()
                .HasKey(x => x.ReadId);

            modelBuilder.Entity<NotificationTarget>()
                .HasKey(x => x.TargetId);


            modelBuilder.Entity<ExamTimeSlot>(entity =>
            {
                entity.HasKey(e => e.ExamTimeSlotId);

                entity.Property(e => e.StartTime)
                      .HasColumnType("time");

                entity.Property(e => e.EndTime)
                      .HasColumnType("time");

                entity.Property(e => e.IsActive)
                      .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");
            });


            modelBuilder.Entity<StudentMarks>()
            .HasIndex(x => new
            {
                x.StudentId,
                x.ClassId,
                x.SectionId,
                x.SubjectId,
                x.ExamId
            })
            .IsUnique();

            modelBuilder.Entity<StudentMarks>()
                .Property(x => x.MarksObtained)
                .HasColumnType("decimal(5,2)");


            // StudentEnrollment
            modelBuilder.Entity<StudentEnrollment>(entity =>
            {
                entity.ToTable("StudentEnrollments");

                entity.HasKey(e => e.EnrollmentId);

                entity.Property(e => e.EnrollmentId)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.StudentId).IsRequired();
                entity.Property(e => e.ClassId).IsRequired();
                entity.Property(e => e.SectionId).IsRequired();
                entity.Property(e => e.AcademicYearId).IsRequired();
                entity.Property(e => e.RollNumber).IsRequired();

                entity.Property(e => e.Status)
                      .HasMaxLength(20)
                      .HasDefaultValue("Active");

                entity.Property(e => e.IsActive)
                      .HasDefaultValue(true);

                entity.Property(e => e.CreatedDate)
                      .HasDefaultValueSql("GETDATE()");

                // Relationships
                entity.HasOne(e => e.Student)
                      .WithMany() // or .WithMany(s => s.Enrollments)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Class)
                      .WithMany()
                      .HasForeignKey(e => e.ClassId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Section)
                      .WithMany()
                      .HasForeignKey(e => e.SectionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AcademicYear)
                      .WithMany()
                      .HasForeignKey(e => e.AcademicYearId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // StudentPromotionLog (from earlier, included here for completeness)
            modelBuilder.Entity<StudentPromotionLog>(entity =>
            {
                entity.ToTable("StudentPromotionLog");

                entity.HasKey(e => e.PromotionId);

                entity.Property(e => e.PromotionId)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.StudentId)
                      .IsRequired();

                entity.Property(e => e.Remarks)
                      .HasMaxLength(255);

                entity.Property(e => e.PromotionDate)
                      .HasDefaultValueSql("GETDATE()");

                // Relationships
                entity.HasOne(e => e.Student)
                      .WithMany() // or .WithMany(s => s.PromotionLogs) if you add collection
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.FromClass)
                      .WithMany()
                      .HasForeignKey(e => e.FromClassId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ToClass)
                      .WithMany()
                      .HasForeignKey(e => e.ToClassId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.FromSection)
                      .WithMany()
                      .HasForeignKey(e => e.FromSectionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ToSection)
                      .WithMany()
                      .HasForeignKey(e => e.ToSectionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.FromAcademicYear)
                      .WithMany()
                      .HasForeignKey(e => e.FromAcademicYearId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ToAcademicYear)
                      .WithMany()
                      .HasForeignKey(e => e.ToAcademicYearId)
                      .OnDelete(DeleteBehavior.Restrict);
            });




        }

    }
}
