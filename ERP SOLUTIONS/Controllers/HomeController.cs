using ERP_SOLUTIONS.Models.Entities;
using ERP_SOLUTIONS.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Collections.Specialized.BitVector32;

namespace ERP_SOLUTIONS.Controllers
{
    public class HomeController : Controller
    {
        private readonly MenuService _menuService;

        public HomeController(MenuService menuService)
        {
            _menuService= menuService;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            //// Fetch only active sections with active items
            //var activeMenus = _dbContext.MenuSections
            //    .Where(s => s.Status)                 // Only active sections
            //    .Include(s => s.Items.Where(i => i.Status)) // Only active items per section
            //    .OrderBy(s => s.Id)                     // Optional: order by Id or Title
            //    .ToList();

            var menus = _menuService.GetActiveMenus();
            return View(menus);

            #region Hard code names
            //var sections = new List<DashboardSection>
            //{
            //    new DashboardSection
            //    {
            //        Title = "Student Management",
            //        Icon = "bi-people",
            //        Color = "blue",
            //        Subtitle = "Tools for Manage student admission",
            //        Items = new List<DashboardItem>
            //        {
            //            new DashboardItem { Description="Complete acedamic and personal record management", Title="Student Record", Icon="bi-person-lines-fill", Url="/Student/Index" },
            //            new DashboardItem { Title = "Enrollment Management", Description = "Manage student admissions and enrollment status.", Icon="bi-journal-check", Url="/Enrollment/Index" },
            //            new DashboardItem { Title = "Parent Portal", Description = "Access and manage parent-related information and communications.", Icon="bi-people-fill", Url="/Parent/Index" },
            //            new DashboardItem { Title = "Document Upload", Description = "Upload and manage student documents.", Icon="bi-file-earmark-arrow-up", Url="/Document/Index" },
            //            new DashboardItem { Title = "Exam Report", Description = "View and manage exam reports for students.", Icon="bi-file-text", Url="/Document/Index" },
            //        }
            //    },

            //    new DashboardSection
            //    {
            //        Title = "Academic Management",
            //        Icon = "bi-mortarboard",
            //        Color = "purple",
            //        Subtitle = "Academic operations & setup",
            //        Items = new List<DashboardItem>
            //        {
            //            new DashboardItem { Title = "Academic Year Setup", Description = "Define academic sessions and year-specific settings.", Icon="bi-calendar", Url="/AcademicYear/Index" },
            //            new DashboardItem { Title = "Classe Schedule", Description = "Manage class schedules and timetables.", Icon="bi-door-open", Url="/Class/Index" },
            //            new DashboardItem { Title = "Subject Assignment", Description = "Assign subjects to teachers and classes.", Icon="bi-book", Url="/Subject/Assign" },
            //            new DashboardItem { Title = "Grade Management", Description = "", Icon="Manage grading systems and student marks.", Url="/Grade/Index" }
            //        }
            //    },

            //    new DashboardSection
            //    {
            //        Title = "Examination System",
            //        Icon = "bi-file-earmark-text",
            //        Color = "orange",
            //        Subtitle = "Manage exams & results",
            //        Items = new List<DashboardItem>
            //        {
            //            new DashboardItem { Title="Exams", Description="Create, schedule, and manage exams.",Icon="bi-pencil-square", Url="/Exam/Index" },
            //            new DashboardItem { Title="Admit Card", Description="Generate student admit cards for exams.",Icon="bi-card-checklist", Url="/Exam/Index" },
            //            new DashboardItem { Title="Results", Description="Publish and manage exam results.", Icon="bi-bar-chart", Url="/Result/Index" },
            //            new DashboardItem {Title="Grade Card", Description="Generate and distribute student grade cards.", Icon="bi-bar-journal", Url="/Result/Index" },
            //            new DashboardItem { Title="Question Bank", Description="Maintain a bank of exam questions.", Icon="bi-question-circle", Url="/Result/Index" },
            //        }
            //    },

            //    new DashboardSection
            //    {
            //        Title = "Attendance & Timetable",
            //        Icon = "bi-clock",
            //        Color = "green",
            //        Subtitle = "Track attendance & schedules",
            //        Items = new List<DashboardItem>
            //        {
            //            new DashboardItem { Title = "Attendance", Description = "", Icon="bi-check2-square", Url="/Attendance/Index" },
            //            new DashboardItem { Title = "Attendance Report", Description = "View and analyze attendance records.", Icon="bi-file-text", Url="/Attendance/Index" },
            //            new DashboardItem { Title = "Timetable", Description = "Manage class timetables", Icon="bi-calendar3", Url="/Timetable/Index" },
            //            new DashboardItem { Title = "Notification", Description = "Notify students/teachers about schedule updates.", Icon="bi-bell", Url="/Timetable/Index" }
            //        }
            //    },

            //    new DashboardSection
            //    {
            //        Title = "Communication",
            //        Icon = "bi-clock",
            //        Color = "green",
            //        Subtitle = "Seemless Communication System",
            //        Items = new List<DashboardItem>
            //        {
            //            new DashboardItem { Title = "Post announcements to students, parents, or staff.", Description = "", Icon="bi-megaphone", Url="/Attendance/Index" },
            //            new DashboardItem { Title = "Event Calender", Description = "Track school events and holidays.", Icon="bi-calendar-event", Url="/Attendance/Index" },
            //            new DashboardItem { Title = "Document Sharing", Description = "Share important files and documents.", Icon="bi-folder-symlink", Url="/Timetable/Index" }
            //        }
            //    },

            //    new DashboardSection
            //    {
            //        Title = "Teacher Management",
            //        Icon = "bi-person-badge",
            //        Color = "teal",
            //        Subtitle = "Manage Teacher and reports",
            //        Items = new List<DashboardItem>
            //        {
            //            new DashboardItem { Title = "Teacher Profile", Description = "Maintain Teacher personal and professional profiles.", Icon="bi-person-workspace", Url="/Teacher/Index" },
            //            new DashboardItem { Title = "Attendance Report", Description = "Monitor teacher attendance", Icon="bi-graph-up", Url="/Teacher/Report" },
            //            new DashboardItem { Title = "Performane Review", Description = "Conduct performance evaluations of Teacher.", Icon="bi-award", Url="/Teacher/Index" },
            //            new DashboardItem { Title = "Leave Management", Description = "Track Teacher leave requests and approvals.", Icon="bi-calendar-minus", Url="/Teacher/Report" },
            //            new DashboardItem { Title = "PayRoll", Description = "Manage salary and compensation records.", Icon="bi-cash-stack", Url="/Teacher/Index" },
            //            new DashboardItem { Title = "Assignment", Description = "Assign responsibilities to Teacher", Icon="bi-journal-check", Url="/Teacher/Report" }
            //        }
            //    },

            //    new DashboardSection
            //    {
            //        Title = "Financial Management",
            //        Icon = "bi-cash-stack",
            //        Color = "red",
            //        Subtitle = "Fees & payment tracking",
            //        Items = new List<DashboardItem>
            //        {
            //            new DashboardItem { Title = "Fee Management", Description = "Manage student fees and invoices.", Icon="bi-wallet2", Url="/Fees/Index" },
            //            new DashboardItem { Title = "Payment Tracking", Description = "Track all payments made by students.", Icon="bi-credit-card", Url="/Payment/Tracking" },
            //            new DashboardItem { Title = "Payment Reminder", Description = "Send reminders for due fees.", Icon="bi-bell", Url="/Payment/Reminder" },
            //            new DashboardItem { Title = "Online Payment", Description = "Enable and manage online payment options.", Icon="bi-wallet-fill", Url="/Payment/Reminder" }
            //        }
            //    },

            //    new DashboardSection
            //    {
            //        Title = "Security & Administration",
            //        Icon = "bi-shield-lock",
            //        Color = "dark",
            //        Subtitle = "User roles & permissions",
            //        Items = new List<DashboardItem>
            //        {
            //            new DashboardItem {  Title = "Role", Description = "Manage user roles and privileges.", Icon="bi-person-lock", Url="/Role/Index" },
            //            new DashboardItem {  Title = "Assign Role", Description = "Assign roles to users", Icon="bi-person-plus", Url="/Role/Assign" },
            //            new DashboardItem {  Title = "Manage User", Description = "Add or remove users from the system.", Icon="bi-people", Url="/User/Index" },
            //            new DashboardItem {  Title = "Complain Box", Description = "Receive and manage complaints or feedback.", Icon="bi-chat-left-text", Url="/User/Index" }
            //        }
            //    }
            //};

            //return View(sections);

            #endregion
        }

        [AllowAnonymous]  // Optional: public About page
        public IActionResult About()
        {
            return View();
        }

        [Authorize]       // Protected page example
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
