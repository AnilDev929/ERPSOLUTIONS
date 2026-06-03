namespace SchoolERP.Services.Interfaces
{
    public interface IAuditService
    {
        //Task LogAsync(string action, string controller);
        Task LogAsync(string action, string controller, string activity = null, string description = null, string status = "Success",
            string logLevel = "Info", Exception ex = null);
    }
}
