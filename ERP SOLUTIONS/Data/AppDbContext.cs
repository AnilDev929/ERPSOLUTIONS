using ERP_SOLUTIONS.Models.DTOS;
using ERP_SOLUTIONS.Models.Entities;
using ERP_SOLUTIONS.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ERP_SOLUTIONS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<MenuSection> MenuSections { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<ClassModel> Classes { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<ClassSection> ClassSections { get; set; }
        public DbSet<CourseFee> CourseFees { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<AcademicYear> AcademicYears { get; set; }

        public DbSet<Hostel> Hostels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<StudentHostelAllocation> StudentHostelAllocations { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentFee> StudentFees { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<RoleMenuAccess> RoleMenuAccess { get; set; }


        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }




        public DbSet<CreateStudentResultDto> CreateStudentResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CreateStudentResultDto>().HasNoKey();


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

        }
    }
}
