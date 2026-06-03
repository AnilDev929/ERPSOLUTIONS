using SchoolERP.Models.Entities;

namespace SchoolERP.Models.ViewModels
{
    public class AuditLogFilterVM
    {
        public string Search { get; set; }
        public string Status { get; set; }
        public string LogLevel { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // PAGINATION
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int TotalRecords { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public List<AuditLog> Logs { get; set; }
    }
}
