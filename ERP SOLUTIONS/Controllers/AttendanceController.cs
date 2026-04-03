using ERP_SOLUTIONS.Models.ViewModels;
using ERP_SOLUTIONS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ERP_SOLUTIONS.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IStudentService studentService, IAttendanceService attendanceService)
        {
            _studentService = studentService;
            _attendanceService = attendanceService;
        }

        // GET: Attendance/Student
        public IActionResult Student(int? subjectId)
        {
            // Load Student attendance data

            //var model = new AttendanceViewModel();

            //if (subjectId.HasValue)
            //{
            //    model.SubjectId = subjectId.Value;
            //    model.Students = _studentService.GetStudentsBySubject(subjectId.Value)
            //                                   .Select(s => new StudentViewModel
            //                                   {
            //                                       Id = s.Id,
            //                                       Name = s.Name
            //                                   }).ToList();
            //}

            // Demo Classes
            ViewBag.Classes = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "10th" },
                new SelectListItem { Value = "2", Text = "9th" },
                new SelectListItem { Value = "3", Text = "8th" }
            };

                    // Demo Subjects
                    ViewBag.Subjects = new List<SelectListItem>
            {
                new SelectListItem { Value = "Math", Text = "Math" },
                new SelectListItem { Value = "Science", Text = "Science" },
                new SelectListItem { Value = "English", Text = "English" }
            };


            // Create a demo AttendanceViewModel
            var model = new AttendanceViewModel
            {
                SubjectId = subjectId ?? 1, // default subject ID
                Date = DateTime.Today,
                Students = new List<StudentViewModel>
                    {
                        new StudentViewModel { Id = 1, Name = "John Doe" },
                        new StudentViewModel { Id = 2, Name = "Jane Smith" },
                        new StudentViewModel { Id = 3, Name = "Michael Johnson" },
                        new StudentViewModel { Id = 4, Name = "Emily Davis" },
                        new StudentViewModel { Id = 5, Name = "William Brown" },
                        new StudentViewModel { Id = 6, Name = "Olivia Wilson" },
                        new StudentViewModel { Id = 7, Name = "James Taylor" },
                        new StudentViewModel { Id = 8, Name = "Sophia Anderson" }
                    }
            };

            return View(model);
        }

        
        // POST: Save attendance
        [HttpPost]
        public IActionResult SaveAttendance(AttendanceViewModel model)
        {
            foreach (var student in model.Students)
            {
                _attendanceService.SaveAttendance(student.Id, model.SubjectId, model.Date, student.Status);
            }

            TempData["Message"] = "Attendance saved successfully!";
            return RedirectToAction("Index", new { subjectId = model.SubjectId });
        }

        // GET: Attendance/Teacher
        public IActionResult Teacher()
        {
            // Load Teacher attendance data
            return View();
        }
    }
}
