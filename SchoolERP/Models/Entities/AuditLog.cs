

namespace SchoolERP.Models.Entities
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }

        public int? UserId { get; set; }
        public string? UserName { get; set; }

        public string? SessionId { get; set; }
        public string? CorrelationId  { get; set; }

        public string? Module { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }

        public string? Activity { get; set; }
        public string? Description { get; set; }

        public string? IPAddress { get; set; }
        public string? Browser { get; set; }
        public string UserAgent { get; set; }
        public string? MachineName { get; set; }
        public string? OperatingSystem { get; set; }
        public string? DeviceType { get; set; }
        public string? Location { get; set; }

        public string? RequestUrl { get; set; }
        public string? HttpMethod { get; set; }

        public string? Status { get; set; }
        public string? LogLevel { get; set; }

        public string? ExceptionMessage { get; set; }
        public string? StackTrace { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
