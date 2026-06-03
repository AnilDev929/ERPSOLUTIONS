using SchoolERP.Data;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;
using System.Net;
using System.Net.Sockets;

/*
         When deployed to production:
            You must use: app.UseForwardedHeaders();
            Otherwise you may log proxy IP instead of real client IP.
         *
         */

namespace SchoolERP.Services.Implementations
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;

        public AuditService(
            AppDbContext context,
            IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        public async Task LogAsync( string action, string controller, string activity = null, string description = null,
            string status = "Success", string logLevel = "Info", Exception ex = null)
        {
            var context = _http.HttpContext;

            if (context == null)
                return;

            var request = context.Request;

            // =========================
            // IP Address
            // =========================
            string ip = context.Connection.RemoteIpAddress?.ToString();

            if (ip == "::1" || ip == "127.0.0.1")
            {
                ip = GetLocalIPAddress();
            }

            // =========================
            // User Agent
            // =========================
            string userAgent = request.Headers["User-Agent"].ToString();

            // =========================
            // Browser + OS
            // =========================
            string browser = GetBrowserName(userAgent);

            string operatingSystem = GetOperatingSystem(userAgent);

            // =========================
            // User
            // =========================
            string user =
                context.User?.Identity?.Name ?? "Anonymous";

            // =========================
            // Machine
            // =========================
            string machineName =
                Environment.MachineName;

            // =========================
            // URL + Method
            // =========================
            string requestUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
            string httpMethod = request.Method;

            // =========================
            // Session
            // =========================
            string sessionId = context.Session?.Id;

            // =========================
            // Correlation Id
            // =========================
            string correlationId = Guid.NewGuid().ToString();

            // =========================
            // Location
            // =========================
            string location = await GetLocationAsync(ip);

            // =========================
            // Exception
            // =========================
            string exceptionMessage = ex?.Message;
            string stackTrace = ex?.StackTrace;

            // =========================
            // Save Audit Log
            // =========================
            var log = new AuditLog
            {
                UserName = user,

                Action = action,
                Controller = controller,

                Activity = activity,
                Description = description,

                Status = status,
                LogLevel = logLevel,

                IPAddress = ip,

                UserAgent = userAgent,
                Browser = browser,
                OperatingSystem = operatingSystem,

                MachineName = machineName,

                RequestUrl = requestUrl,
                HttpMethod = httpMethod,

                Location = location,

                ExceptionMessage = exceptionMessage,
                StackTrace = stackTrace,

                SessionId = sessionId,
                CorrelationId = correlationId,

                CreatedOn = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task LogAsync(string action, string controller)
        {
            var context = _http.HttpContext;

            string ip = context?.Connection?.RemoteIpAddress?.ToString();

            if (ip == "::1" || ip == "127.0.0.1")
            {
                ip = GetLocalIPAddress();
            }

            string userAgent = context?.Request?.Headers["User-Agent"].ToString();

            string browser = GetBrowserName(userAgent);

            string user = context?.User?.Identity?.Name ?? "Anonymous";

            string machineName = Environment.MachineName;

            string location = await GetLocationAsync(ip);

            var log = new AuditLog
            {
                UserName = user,
                Action = action,
                Controller = controller,
                IPAddress = ip,
                MachineName = machineName,
                Browser = browser,
                Location = location
            };

            _context.AuditLogs.Add(log);

            await _context.SaveChangesAsync();
        }

        private string GetBrowserName(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return "Unknown";

            userAgent = userAgent.ToLower();

            if (userAgent.Contains("edg"))
                return "Microsoft Edge";

            if (userAgent.Contains("chrome"))
                return "Google Chrome";

            if (userAgent.Contains("firefox"))
                return "Mozilla Firefox";

            if (userAgent.Contains("safari")
                && !userAgent.Contains("chrome"))
                return "Safari";

            if (userAgent.Contains("opr")
                || userAgent.Contains("opera"))
                return "Opera";

            return "Other";
        }

        private string GetOperatingSystem(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return "Unknown";

            userAgent = userAgent.ToLower();

            if (userAgent.Contains("windows"))
                return "Windows";

            if (userAgent.Contains("android"))
                return "Android";

            if (userAgent.Contains("iphone")
                || userAgent.Contains("ipad"))
                return "iOS";

            if (userAgent.Contains("mac"))
                return "MacOS";

            if (userAgent.Contains("linux"))
                return "Linux";

            return "Other";
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "Unknown";
        }

        private async Task<string> GetLocationAsync(string ip)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ip)
                    || ip == "::1"
                    || ip == "127.0.0.1")
                {
                    ip = GetLocalIPAddress();
                    return $"Local Network ({ip})";
                }

                using var client = new HttpClient();

                var response =
                    await client.GetStringAsync(
                        $"http://ip-api.com/json/{ip}");

                dynamic data =
                    Newtonsoft.Json.JsonConvert.DeserializeObject(response);

                return $"{data.city}, {data.country}";
            }
            catch
            {
                return "Unknown";
            }
        }

    }
}
