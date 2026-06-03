using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Data;
using SchoolERP.Services.Implementations;
using SchoolERP.Services.Interfaces;

namespace SchoolERP.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class BackupController : Controller
    {
        private readonly DatabaseBackupService _backupService;
        private readonly AppDbContext _context;
        private readonly IAuditService _auditService;
        //private readonly ILogger _logger;

        public BackupController(IAuditService audit,
            DatabaseBackupService backupService,
            AppDbContext context)
        {
            _backupService = backupService;
            _context = context;
            _auditService = audit;
        }

        public IActionResult Index()
        {
            var backups = _context.DatabaseBackups
                .OrderByDescending(x => x.CreatedOn)
                .ToList();

            return View(backups);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBackup()
        {
            try{
                var response = await _backupService.CreateBackupAsync(User.Identity.Name);
                if (response.Success)
                {
                    TempData["Success"] = $"Database backup created successfully at: {response.Message}";
                }
                else {
                    TempData["Error"] = response.Message;
                }

                await _auditService.LogAsync(action: "CreateBackup",
                    controller: "Backup",
                    activity: "Backup Created",
                    description: "New Database backup created successfully"
                );

                return Json(new
                {
                    success = true,
                    message = "Database backup created successfully"
                });
            }
            catch (Exception ex)
            {
                await _auditService.LogAsync(
                    action: "CreateBackup",
                    controller: "Backup",
                    activity: "Failed to backup",
                    description: "Unable to take backup of database",
                    status: "Failed",
                    logLevel: "Warning",
                    ex: ex
                );

                TempData["Error"] =
                    "Backup failed. Please contact administrator.";

                //// Log real error internally
                //_logger.LogError(ex, "Database backup failed");

                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });

            }
        }

        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var backup = await _context.DatabaseBackups
                .FindAsync(id);

            if (backup == null)
                return NotFound();

            await _backupService
                .RestoreDatabaseAsync(backup.FilePath);

            TempData["success"] =
                "Database restored successfully";


//            await _auditService.LogAsync(
//    action: "Logout",
//    controller: "Account",
//    activity: "User Logout",
//    description: "User logged out"
//);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Download(int id)
        {
            var backup = _context.DatabaseBackups.Find(id);

            byte[] bytes =
                System.IO.File.ReadAllBytes(backup.FilePath);

            return File(bytes,
                "application/octet-stream",
                backup.FileName);
        }

        

        
    }
}
