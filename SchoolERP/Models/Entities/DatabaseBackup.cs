using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.Entities
{
    public class DatabaseBackup
    {
        [Key]
        public int BackupId { get; set; }

        public string? FileName { get; set; }
        public string? FilePath { get; set; }

        public decimal BackupSizeMB { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public string? Status { get; set; }

        public string? Remarks { get; set; }
    }
}
