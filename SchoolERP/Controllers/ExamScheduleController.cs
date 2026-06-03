using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using System.Security.Claims;

namespace SchoolERP.Controllers
{
    public class ExamScheduleController : Controller
    {
        private readonly AppDbContext _dbContext;

        public ExamScheduleController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> TimeSlots(int? examTypeId)
        {
            var examTypes = await _dbContext.Exams
            .OrderBy(x => x.ExamId)
            .ToListAsync();

            var query = _dbContext.ExamTimeSlots.AsQueryable();

            if (examTypeId.HasValue)
            {
                query = query.Where(x => x.ExamId == examTypeId.Value);
            }

            var startDate = new DateTime(DateTime.Now.Year, 1, 1);
            var endDate = startDate.AddYears(1);

            var data = await
                //_dbContext.ExamTimeSlots
                query
                .Where(x => x.ExamDate >= startDate && x.ExamDate < endDate)
                .OrderBy( x => x.ExamId)
                .ThenBy(x => x.StartTime)
                .ToListAsync();

            ViewBag.ExamTypes = examTypes;
            ViewBag.ExamType = examTypeId;
            return View(data);
        }

        [HttpGet]
        public async Task<JsonResult> CheckTimeConflict(
            TimeSpan startTime, TimeSpan endTime, string roomNo, string floorNo)
        {
            bool exists = await _dbContext.ExamTimeSlots
                .AnyAsync(x =>
                    x.RoomNo == roomNo &&
                    x.FloorNo == floorNo &&
                    (
                        startTime < x.EndTime &&
                        endTime > x.StartTime
                    )
                );

            return Json(exists);
        }

        [HttpGet]
        public async Task<IActionResult> CreateTimeSlot(int? id)
        {
            var examTypes = await _dbContext.Exams
                .OrderBy(x => x.ExamId)
                .ToListAsync();

            ViewBag.ExamTypes = examTypes;

            ExamTimeSlotVM vm = new ExamTimeSlotVM();
            if (id.HasValue)
            {
                var entity = await _dbContext.ExamTimeSlots
                    .FirstOrDefaultAsync(x => x.ExamTimeSlotId == id.Value);

                if (entity != null)
                {
                    vm.Id = entity.ExamTimeSlotId;
                    vm.ExamId = entity.ExamId;
                    vm.ExamDate = entity.ExamDate;
                    vm.StartTime = entity.StartTime;
                    vm.EndTime = entity.EndTime;
                    vm.FloorNumber = entity.FloorNo;
                    vm.RoomNumber = entity.RoomNo;
                    vm.IsActive = entity.IsActive;
                }
            }

            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> CreateTimeSlot(ExamTimeSlotVM vm)
        {
            if (!ModelState.IsValid)
            {
                var examTypes = await _dbContext.Exams
                    .OrderBy(x => x.ExamId)
                    .ToListAsync();

                var startDate = new DateTime(DateTime.Now.Year, 1, 1);
                var endDate = startDate.AddYears(1);

                var timeSlots = await _dbContext.ExamTimeSlots
                    .Where(x => x.ExamDate >= startDate &&
                                x.ExamDate < endDate)
                    .OrderBy(x => x.ExamDate)
                    .ThenBy(x => x.StartTime)
                    .ToListAsync();

                ViewBag.ExamTypes = examTypes;

                // return SAME VIEW
                return View("TimeSlots", timeSlots);
            }

            if (ModelState.IsValid)
            {
                bool conflictExists = await _dbContext.ExamTimeSlots
                    .AnyAsync(x =>

                        x.RoomNo == vm.RoomNumber &&
                        x.FloorNo == vm.FloorNumber &&
                        (
                            vm.StartTime < x.EndTime &&
                            vm.EndTime > x.StartTime
                        )
                    );

                if (conflictExists)
                {
                    ModelState.AddModelError(string.Empty,
                        "Time slot conflicts with an existing slot in the same room.");
                }
                else
                {
                    // 🔥 EDIT CASE
                    if (vm.Id > 0)
                    {
                        var entity = await _dbContext.ExamTimeSlots
                            .FirstOrDefaultAsync(x => x.ExamTimeSlotId == vm.Id);

                        if (entity == null)
                            return NotFound();

                        entity.ExamDate = vm.ExamDate;
                        entity.StartTime = vm.StartTime;
                        entity.EndTime = vm.EndTime;
                        entity.ExamId = vm.ExamId;
                        entity.FloorNo = vm.FloorNumber;
                        entity.RoomNo = vm.RoomNumber;
                        entity.IsActive = vm.IsActive;

                        _dbContext.ExamTimeSlots.Update(entity);
                    }
                    else
                    {
                        // 🆕 CREATE CASE
                        ExamTimeSlot examTimeSlot = new ExamTimeSlot();
                        examTimeSlot.ExamDate = vm.ExamDate;
                        examTimeSlot.StartTime = vm.StartTime;
                        examTimeSlot.EndTime = vm.EndTime;
                        //examTimeSlot.SlotName = vm.SlotName;
                        examTimeSlot.ExamId = vm.ExamId;
                        examTimeSlot.FloorNo = vm.FloorNumber;
                        examTimeSlot.RoomNo = vm.RoomNumber;
                        examTimeSlot.IsActive = vm.IsActive;

                        _dbContext.ExamTimeSlots.Add(examTimeSlot);
                    }
                        
                    await _dbContext.SaveChangesAsync();

                    TempData["success"] = "Time Slot Created Successfully";

                    return RedirectToAction(nameof(TimeSlots));
                }
            }

            return RedirectToAction("TimeSlots");
        }

        public ActionResult Index(int? classId, int? examId)
        {
            var schedules = _dbContext.ExamSchedules
                .Include(x => x.Subject)
                .Include(x => x.Class)
                .Include(x => x.Exam)
                .Include(x => x.TimeSlot)
                .AsQueryable();

            if (classId.HasValue)
            {
                schedules = schedules.Where(x => x.ClassId == classId.Value);
            }

            if (examId.HasValue)
            {
                schedules = schedules.Where(x => x.ExamId == examId.Value);
            }

            var classes = _dbContext.Classes.ToList();
            var exams = _dbContext.Exams.ToList();

            ViewBag.Classes = new SelectList(classes, "ClassId", "ClassName", classId);
            ViewBag.Exams = new SelectList(exams, "ExamId", "ExamName", examId);

            //return View(schedules);
            return View(schedules.OrderBy(x => x.ExamDate).ToList());
        }

        protected int GetLoggedInUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim == null)
                throw new Exception("Sorry, you’re not authorized to access this page.");

            // convert to int if needed
            int userId = int.Parse(claim);
            return userId;
        }

        public ActionResult Create()
        {
            var activeYear = _dbContext.AcademicYears
                        .FirstOrDefault(x => x.IsActive);

            ViewBag.AcademicYear = activeYear?.YearName ?? "Not Set";

            var vm = new ExamScheduleViewModel
            {
                Exams = _dbContext.Exams.Select(x => new SelectListItem
                {
                    Value = x.ExamId.ToString(),
                    Text = x.ExamName
                }).ToList(),

                Classes = _dbContext.Classes.Select(x => new SelectListItem
                {
                    Value = x.ClassId.ToString(),
                    Text = x.ClassName
                }).ToList(),

               
                Subjects = new List<SubjectScheduleVM>()
            };

            return View(vm);
        }

        //// AJAX: Load Sections by Class
        //public async Task<IActionResult> GetSectionsByClass(int classId)
        //{
        //    var teacherId = GetLoggedInTeacherId();

        //    var sections = await _dbContext.ClassSectionSubject
        //                    .Where(x =>
        //                        x.TeacherId == teacherId &&
        //                        x.ClassSection.ClassId == classId)
        //                    .Select(x => new
        //                    {
        //                        x.ClassSection.Section.SectionId,
        //                        x.ClassSection.Section.SectionName,
        //                        x.ClassSectionId
        //                    })
        //                    .Distinct()
        //                    .ToListAsync();

        //    return Json(sections);
        //}

        // AJAX: Load Subjects by Class
        public JsonResult GetSubjectsByClass(int classId)
        {

            var subjects = (from cs in _dbContext.ClassSections
                            join css in _dbContext.ClassSectionSubject
                                on cs.Id equals css.ClassSectionId
                            join sub in _dbContext.Subjects
                                on css.SubjectId equals sub.SubjectId
                            where cs.ClassId == classId
                                  && sub.IsActive
                            select new ClassSubjectDto
                            {
                                SubjectId = sub.SubjectId,
                                SubjectName = sub.SubjectName,
                                SubjectCode = sub.SubjectCode
                            })
                    .GroupBy(x => x.SubjectId)
                    .Select(g => g.First())
                    .ToList();

            //var subjects = (from css in _dbContext.ClassSectionSubject
            //              join cs in _dbContext.ClassSections on css.ClassSectionId equals cs.Id
            //              join c in _dbContext.Classes on cs.ClassId equals c.ClassId
            //              join s in _dbContext.Sections on cs.SectionId equals s.SectionId
            //              join sub in _dbContext.Subjects on css.SubjectId equals sub.SubjectId
            //              where cs.ClassId == classId
            //                 && sub.IsActive
            //              select new ClassSubjectDto
            //              {
            //                  SubjectId = sub.SubjectId,
            //                  SubjectName = sub.SubjectName,
            //                  SubjectCode = sub.SubjectCode,
            //                  ClassName = c.ClassName,
            //                  SectionName = s.SectionName
            //              }).ToList();

            return Json(subjects);
        }

        //Get available slo
        public JsonResult GetAvailableSlots(DateTime examDate, int examId)
        {
            var usedSlots = _dbContext.ExamSchedules
                .Where(x => x.ExamDate.Date == examDate.Date && x.ExamId == examId)
                .Select(x => x.ExamTimeSlotId)
                .ToList();

            var slots = _dbContext.ExamTimeSlots
                .Where(x => x.IsActive && x.ExamId == examId)
                .AsEnumerable()   // 👈 important fix
                .Select(x => new
                {
                    x.ExamTimeSlotId,
                    x.SlotName,
                    IsDisabled = usedSlots.Contains(x.ExamTimeSlotId)
                })
                .ToList();

            return Json(slots);
        }


        [HttpPost]
        public ActionResult Create(ExamScheduleViewModel vm)
        {
            if (vm.Subjects == null || !vm.Subjects.Any())
            {
                ModelState.AddModelError("", "No subjects found.");
                return View(vm);
            }

            // 1️⃣ VALIDATION
            for (int i = 0; i < vm.Subjects.Count; i++)
            {
                var sub = vm.Subjects[i];

                // Required field validation
                if (sub.ExamDate == null)
                { ModelState.AddModelError($"Subjects[{i}].ExamDate", "Exam date is required."); } 
                if (sub.MaxMarks == null) { ModelState.AddModelError($"Subjects[{i}].MaxMarks", "Max marks are required."); } 
                // Business rule validation
                if (sub.ExamDate != null && sub.ExamDate < DateTime.Today) 
                { ModelState.AddModelError($"Subjects[{i}].ExamDate", "Exam date cannot be in the past."); }
            }

            // Stop if validation fails
            if (!ModelState.IsValid)
                return View(vm);

            try
            {

                // =========================
                // EDIT MODE
                // =========================
                if (vm.Id.HasValue)
                {
                    var sub = vm.Subjects.First();

                    // Get ALL related rows
                    var existingList = _dbContext.ExamSchedules
                        .Where(x => x.ClassId == vm.ClassId
                                 && x.ExamId == vm.ExamId)
                        .ToList();

                    if (!existingList.Any())
                    {
                        ModelState.AddModelError("", "No records found.");
                        return View(vm);
                    }

                    // OPTIONAL: remove old rows (clean approach)
                    _dbContext.ExamSchedules.RemoveRange(existingList);

                    // recreate updated rows
                    var updated = vm.Subjects.Select(s => new ExamSchedule
                    {
                        ExamId = vm.ExamId,
                        ClassId = vm.ClassId,
                        SubjectId = s.SubjectId,
                        ExamDate = s.ExamDate,
                        MaxMarks = s.MaxMarks,
                        PassingMarks = s.PassingMarks,
                        ExamTimeSlotId = s.ExamTimeSlotId
                    }).ToList();

                    _dbContext.ExamSchedules.AddRange(updated);

                    _dbContext.SaveChanges();

                    TempData["Success"] = "Exam schedule updated successfully.";
                }
                else
                {
                    // =========================
                    // CREATE MODE
                    // =========================

                    // 2️⃣ FETCH EXISTING SUBJECT IDS (ONE DB CALL ONLY)
                    var subjectIds = vm.Subjects.Select(s => s.SubjectId).ToList();

                    var existingSubjectIds = _dbContext.ExamSchedules
                        .Where(x => x.ClassId == vm.ClassId && x.ExamId == vm.ExamId)
                        .Select(x => x.SubjectId)
                        .ToList();

                    // 3️⃣ CHECK DUPLICATES IN MEMORY (FAST)
                    var duplicateSubjects = subjectIds.Intersect(existingSubjectIds).ToList();

                    if (duplicateSubjects.Any())
                    {
                        ModelState.AddModelError("", "Some subjects already have schedules.");
                        return View(vm);
                    }

                    // 4️⃣ MAP DATA (NO LOOP SIDE EFFECTS)
                    var schedules = vm.Subjects.Select(sub => new ExamSchedule
                    {
                        ExamId = vm.ExamId,
                        ClassId = vm.ClassId,
                        SubjectId = sub.SubjectId,
                        ExamDate = sub.ExamDate,
                        MaxMarks = sub.MaxMarks,
                        PassingMarks = sub.PassingMarks,
                        ExamTimeSlotId = sub.ExamTimeSlotId
                    }).ToList();

                    // 5️⃣ BULK INSERT
                    _dbContext.ExamSchedules.AddRange(schedules);
                    _dbContext.SaveChanges();

                    TempData["Success"] = "Exam schedule saved successfully.";
                }
                return RedirectToAction("Index", new { classId = (int?)null, examId = (int?)null }); 
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Duplicate schedule detected or DB error.");
                return View(vm);
            }
        
        }


        //Edit Schedule Screen
        public ActionResult Edit(int id)
        {
            var schedule = _dbContext.ExamSchedules
                .FirstOrDefault(x => x.ScheduleId == id);

            if (schedule == null)
                return NotFound();
             
            var vm = new ExamScheduleViewModel
            {
                Id = schedule.ScheduleId,
                ExamId = schedule.ExamId,
                ClassId = schedule.ClassId,
                Subjects = new List<SubjectScheduleVM>
                {
                    new SubjectScheduleVM
                    {
                        SubjectId = schedule.SubjectId,
                        ExamDate = schedule.ExamDate,
                        MaxMarks = schedule.MaxMarks,
                        PassingMarks = schedule.PassingMarks,
                        ExamTimeSlotId = schedule.ExamTimeSlotId
                    }
                }
            };

            return View("Create", vm); // 🔥 REUSE CREATE VIEW
        }

        [HttpPost]
        public ActionResult Edit(ExamScheduleViewModel vm)
        {
            var existing = _dbContext.ExamSchedules
                .Where(x => x.ExamId == vm.ExamId && x.ClassId == vm.ClassId)
                .ToList();

            _dbContext.ExamSchedules.RemoveRange(existing);

            foreach (var sub in vm.Subjects)
            {
                if (sub.ExamDate != null && sub.MaxMarks != null)
                {
                    _dbContext.ExamSchedules.Add(new ExamSchedule
                    {
                        ExamId = vm.ExamId,
                        ClassId = vm.ClassId,
                        SubjectId = sub.SubjectId,
                        ExamDate = sub.ExamDate,
                        MaxMarks = sub.MaxMarks
                    });
                }
            }

            _dbContext.SaveChanges();

            TempData["Success"] = "Schedule updated successfully!";
            return RedirectToAction("Create");
        }



        [HttpGet]
        public async Task<IActionResult> StudentExamSchedule(int examId = 0)
        {
            var exams = await _dbContext.Exams
                .Where(x => x.IsActive)
                .OrderBy(x => x.StartDate)
                .ToListAsync();

            var query =
                from es in _dbContext.ExamSchedules

                join sub in _dbContext.Subjects
                    on es.SubjectId equals sub.SubjectId

                join ex in _dbContext.Exams
                    on es.ExamId equals ex.ExamId

                join ts in _dbContext.ExamTimeSlots
                    on es.ExamTimeSlotId equals ts.ExamTimeSlotId

                select new StudentExamScheduleVM
                {
                    ScheduleId = es.ScheduleId,
                    SlotName = ts.SlotName,
         
                    ExamId = ex.ExamId,
                    ExamName = ex.ExamName,

                    SubjectId = sub.SubjectId,
                    SubjectName = sub.SubjectName,

                    ExamDate = es.ExamDate,

                    StartTime = ts.StartTime,
                    EndTime = ts.EndTime,

                    RoomNo = ts.RoomNo,
                    FloorNo = ts.FloorNo,

                    MaxMarks = es.MaxMarks,
                    PassingMarks = es.PassingMarks
                };

            if (examId > 0)
            {
                query = query.Where(x => x.ExamId == examId);
            }

            var schedules = await query
                .OrderBy(x => x.ExamDate)
                .ThenBy(x => x.StartTime)
                .ToListAsync();

            ViewBag.Exams = exams;
            ViewBag.SelectedExamId = examId;

            return View(schedules);
        }

    }
}
