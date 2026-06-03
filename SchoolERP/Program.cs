
using SchoolERP.Data;
using SchoolERP.Helpers;
using SchoolERP.Services.Implementations;
using SchoolERP.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
//Services registered before Build()

// Add logging providers (optional, Console is default)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();   // Logs to console
builder.Logging.AddDebug();     // Logs to Visual Studio debug output

// =====================
// 1 REGISTER SERVICES
// =====================

// MVC (UI + Controllers)
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();  // required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// ✅ Add Cookie Authentication (for MVC login)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";        // Redirect here if unauthorized
        options.AccessDeniedPath = "/Account/AccessDenied"; // Optional
    });

//AppDomain.CurrentDomain.SetData(
//    "DataDirectory",
//    Path.Combine(builder.Environment.ContentRootPath, "App_Data"));

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IStudentProfileService, StudentProfileService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IFeeConfigurationService, FeeConfigurationService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IHostelService, HostelService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<ICourseFeeService, CourseFeeService>();

builder.Services.AddScoped<IHolidayService, HolidayService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IFeesService, FeesService>();
builder.Services.AddScoped<IGradeRuleService, GradeRuleService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IMarksService, MarksService>();

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<StudentPromotionService>();
builder.Services.AddScoped<DatabaseBackupService>();

builder.Services.AddScoped<IPromotionService, PromotionService>();

builder.Services.AddScoped<MenuService>(); //One instance per HTTP request
// Helpers
builder.Services.AddScoped<JwtHelper>(); // keep for future JWT use


builder.Services.AddAuthorization();
// Session + HTTP
builder.Services.AddSession();
builder.Services.AddHttpClient();

//Register memory cache
builder.Services.AddMemoryCache();

// Swagger (for future API use)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// =====================
// BUILD APP
// =====================

//Always put your middleware like this:
var app = builder.Build();

// =====================
// MIDDLEWARE ORDER (FIXED)
// =====================

if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Account/Error");
    app.UseHsts();

    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting(); // ✅ FIRST establish routing

// Authentication & Authorization MUST come before MVC endpoints
app.UseAuthentication(); // ✅ must come before Authorization
app.UseAuthorization();

app.UseSession();

// Add cache-control headers here, after auth but before endpoints
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

// =====================
// ROUTING
// =====================
// Redirect root URL "/" to "/Home"
app.MapGet("/", context =>
{
    context.Response.Redirect("/Home");
    return Task.CompletedTask;
});

// Default route → Login page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=WelComePage}/{id?}"
);

// API Controllers (for future use)
app.MapControllers();

app.Run();