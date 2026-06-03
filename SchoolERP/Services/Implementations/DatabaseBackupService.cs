using Microsoft.Data.SqlClient;
using SchoolERP.Common;
using SchoolERP.Data;
using SchoolERP.Models.Entities;

namespace SchoolERP.Services.Implementations
{
    public class DatabaseBackupService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public DatabaseBackupService(  IConfiguration config,  AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task<ServiceResponse> CreateBackupAsync(string userName)
        {
            try
            {
                string backupFolder = _config["BackupSettings:BackupPath"];

                //string backupFolder = Path.Combine(Directory.GetCurrentDirectory(), "Backups", "Database");

                // check disk
                if (!Directory.Exists(backupFolder))
                {
                    return ServiceResponse.Fail("Backup drive not found.");
                }

                if (!Directory.Exists(backupFolder))
                    Directory.CreateDirectory(backupFolder);

                //string dbName =
                //    _config.GetConnectionString("DefaultConnection")
                //    .Split(';')
                //    .FirstOrDefault(x => x.StartsWith("Initial Catalog"))
                //    ?.Split('=')[1] ?? "ERP_SchoolDB";

                string fileName = $"ERP_SchoolDB_{DateTime.Now:yyyyMMdd_HHmmss}.bak";

                string fullPath = Path.Combine(backupFolder, fileName);

                string connectionString = _config.GetConnectionString("DefaultConnection");

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
                string dbName = builder.InitialCatalog;

                using SqlConnection con = new SqlConnection(connectionString);

                string query = $@"
                BACKUP DATABASE [{dbName}]
                TO DISK = '{fullPath}'
                WITH FORMAT, INIT,
                NAME = 'School ERP Backup'";

                using SqlCommand cmd = new SqlCommand(query, con);

                await con.OpenAsync();

                await cmd.ExecuteNonQueryAsync();

                FileInfo fileInfo = new FileInfo(fullPath);

                var backup = new DatabaseBackup
                {
                    FileName = fileName,
                    FilePath = fullPath,
                    BackupSizeMB =
                        Math.Round(fileInfo.Length / 1024m / 1024m, 2),

                    CreatedBy = userName,
                    CreatedOn = DateTime.Now,
                    Status = "Success"
                };

                _context.DatabaseBackups.Add(backup);

                await _context.SaveChangesAsync();

                //await DeleteOldBackupsAsync();
                await DeleteOldBackupsByDaysAsync(30);

                return ServiceResponse.Ok(fullPath);
            }
            catch (SqlException)
            {
                return ServiceResponse.Fail("Database connection failed.");
            }
            catch (Exception ex)
            {
                return ServiceResponse.Fail(ex.Message);
            }
        }

        public async Task RestoreDatabaseAsync(string backupFile)
        {
            string connectionString = _config.GetConnectionString("DefaultConnection");

            SqlConnectionStringBuilder builder =
                new SqlConnectionStringBuilder(connectionString);

            string dbName = builder.InitialCatalog;

            builder.InitialCatalog = "master";

            using SqlConnection con =
                new SqlConnection(builder.ConnectionString);

            string query = $@"
    ALTER DATABASE [{dbName}]
    SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

    RESTORE DATABASE [{dbName}]
    FROM DISK = '{backupFile}'
    WITH REPLACE;

    ALTER DATABASE [{dbName}]
    SET MULTI_USER;";

            using SqlCommand cmd =
                new SqlCommand(query, con);

            await con.OpenAsync();

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteOldBackupsAsync(int keepLast = 30)
        {
            var oldBackups = _context.DatabaseBackups
                .OrderByDescending(x => x.CreatedOn)
                .Skip(keepLast)
                .ToList();

            foreach (var backup in oldBackups)
            {
                if (File.Exists(backup.FilePath))
                {
                    File.Delete(backup.FilePath);
                }

                _context.DatabaseBackups.Remove(backup);
            }

            await _context.SaveChangesAsync();
        }


        public async Task DeleteOldBackupsByDaysAsync(int days = 30)
        {
            DateTime cutoffDate = DateTime.Now.AddDays(-days);

            // Get latest backup
            var latestBackup = _context.DatabaseBackups
                .OrderByDescending(x => x.CreatedOn)
                .FirstOrDefault();

            // Get old backups except latest one
            var oldBackups = _context.DatabaseBackups
                .Where(x => x.CreatedOn < cutoffDate
                    && x.BackupId != latestBackup.BackupId)
                .ToList();

            foreach (var backup in oldBackups)
            {
                if (System.IO.File.Exists(backup.FilePath))
                {
                    System.IO.File.Delete(backup.FilePath);
                }

                _context.DatabaseBackups.Remove(backup);
            }

            await _context.SaveChangesAsync();
        }

        //public async Task DeleteOldBackupsByDaysAsync(int days = 30)
        //{
        //    DateTime cutoffDate = DateTime.Now.AddDays(-days);

        //    var oldBackups = _context.DatabaseBackups
        //        .Where(x => x.CreatedOn < cutoffDate)
        //        .ToList();

        //    foreach (var backup in oldBackups)
        //    {
        //        if (File.Exists(backup.FilePath))
        //        {
        //            File.Delete(backup.FilePath);
        //        }

        //        _context.DatabaseBackups.Remove(backup);
        //    }

        //    await _context.SaveChangesAsync();
        //}
    }
}
