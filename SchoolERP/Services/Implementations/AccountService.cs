using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Services.Interfaces;
using System.Data;
using System.Text.Json.Serialization.Metadata;

namespace SchoolERP.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SubjectService> _logger;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountService(AppDbContext context, 
            ILogger<SubjectService> logger
            )
        {
            _context = context;
            _logger = logger;
            _passwordHasher = new PasswordHasher<User>();
        }

        public List<SelectListItem> GetRolesDropdown()
        {
            return _context.Roles
                .Where(r => r.IsActive && r.RoleName.ToLower() != "parent")
                .Select(r => new SelectListItem
                {
                    Value = r.RoleID.ToString(),
                    Text = r.RoleName
                }).ToList();
        }

        public async Task<(bool Success, string Message)> LoginAsync(string username, string password, int roleid)
        {
            try
            {
                // 1. Get user from DB
                var user = await _context.Users
                   .Include(u => u.UserRoles)
                       .ThenInclude(ur => ur.Role)
                   .FirstOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower());

                if (user == null)
                {
                    return (false, "Invalid username or password");
                }

                //// 2. Verify password
                //var hasher = new PasswordHasher<User>();
                //var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

                //if (result != PasswordVerificationResult.Success)
                //{
                //    return (false, "Invalid username or password");
                //}

                // 2️⃣ Check if account is active / locked
                if (!user.IsActive)
                    return (false, "Account is inactive");

                if (user.LockoutUntil != null && user.LockoutUntil > DateTime.Now)
                    return (false, $"Account is locked until {user.LockoutUntil}");

                // 3️⃣ Verify password
                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
                if (result != PasswordVerificationResult.Success)
                {
                    // Increment failed login count
                    user.FailedLoginCount++;

                    // 🔴 Lock user after 5 failed attempts
                    if (user.FailedLoginCount >= 5)
                    {
                        user.LockoutUntil = DateTime.Now.AddMinutes(15); // lock for 15 min
                        user.FailedLoginCount = 0; // reset after lock

                        await _context.SaveChangesAsync();
                        return (false, "Account locked due to multiple failed login attempts. Try after 15 minutes.");
                    }

                    await _context.SaveChangesAsync();
                    return (false, $"Invalid username or password. Attempt {user.FailedLoginCount}/5");
                }

                // 4️⃣ Check Role
                bool hasRole = user.UserRoles.Any(ur => ur.Role.RoleID == roleid);
                if (!hasRole)
                    return (false, "User does not have this role");

                // 5️⃣ Reset failed login count & update last login
                user.FailedLoginCount = 0;
                user.LockoutUntil = null;

                // Shift current login to previous
                user.PreviousLoginAt = user.LastLoginAt;

                // Set new login time
                user.LastLoginAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return (true, "Login successful");
            }
            catch (Exception)
            {
                return (false, "Login Failed");
            }
        }

        private string HashPassword(LoginModel user, string password)
        {
            var hasher = new PasswordHasher<object>();
            return hasher.HashPassword(user, password);
        }

        //Parent Login 
        public async Task<(int Success, string Message)> ParentLogin(LoginModel login)
        {
            //-----------------------------------
            // Find User
            //-----------------------------------
            var user = await _context.Parents
                .FirstOrDefaultAsync(x =>
                    (x.ParentEmail == login.UserName ||
                     x.Phone == login.UserName)
                     && x.IsActive);

            //-----------------------------------
            // User Not Found
            //-----------------------------------
            if (user == null)
            {
                return (0, "User not found"); 
            }

            //-----------------------------------
            // Account Locked
            //-----------------------------------
            if (user.IsLocked)
            {
                return (0, "Account locked. Contact school.");
            }
            
            //-----------------------------------
            // Verify Password
            //-----------------------------------
            var hasher = new PasswordHasher<object>();

            var result = hasher.VerifyHashedPassword(
                null,
                user.PasswordHash,
                login.Password
            );

            //------------------------------------
            // Input Parameters
            //------------------------------------
            var loginIdParam = new SqlParameter("@LoginId", login.UserName);
            var passwordParam = new SqlParameter("@PasswordHash",
                result != PasswordVerificationResult.Success ? login.Password : user.PasswordHash);

            //------------------------------------
            // Output Parameters
            //------------------------------------
            var parentIdParam = new SqlParameter
            {
                ParameterName = "@ParentId",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            var statusParam = new SqlParameter
            {
                ParameterName = "@Status",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            var messageParam = new SqlParameter
            {
                ParameterName = "@Message",
                SqlDbType = SqlDbType.NVarChar,
                Size = 200,
                Direction = ParameterDirection.Output
            };

            try
            {
                //------------------------------------
                // Execute Stored Procedure
                //------------------------------------
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_ParentLogin " +
                    "@LoginId, " +
                    "@PasswordHash, " +
                    "@ParentId OUTPUT, " +
                    "@Status OUTPUT, " +
                    "@Message OUTPUT",

                    loginIdParam,
                    passwordParam,
                    parentIdParam,
                    statusParam,
                    messageParam
                );

                //------------------------------------
                // Read Output Values
                //------------------------------------
                int status = Convert.ToInt32(statusParam.Value);
                string message = messageParam.Value?.ToString();
                int parentId = parentIdParam.Value != DBNull.Value
                    ? Convert.ToInt32(parentIdParam.Value)
                    : 0;
                if(status == 1)
                {
                    message = parentId.ToString();
                }
                return (status, message);
            }
            catch(Exception ex)
            {
                return (-1, ex.Message);
            }
           
        }

        public async Task<User> GetParentDtl(int parentID)
        {
            var user = await _context.Parents
               .Where(x => x.ParentId == parentID && x.IsActive)
               .Select(x => new User
               {
                   UserID = x.ParentId,
                   UserName = x.ParentEmail,
                   FullName = x.FatherName,
                   Email = x.ParentEmail,
                   MobileNo = x.EmergencyContact,
                   LastLoginAt = x.LastLoginAt,
                   PreviousLoginAt = x.PreviousLoginAt
               })
               .FirstOrDefaultAsync();

            return user;
        }


        public async Task<UserInfoDTO> GetUserInfoAsync(string username)
        {
            try
            {
                // 1. Get user from DB
                var user = await _context.Users
                     .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                     .Select(u => new UserInfoDTO
                     {
                         UserID = u.UserID,
                         RoleID = u.UserRoles.Select(ur => ur.Role.RoleID).FirstOrDefault(),
                         UserName = u.UserName,
                         FullName = u.FullName ?? "N/A",
                         Email = u.Email,
                         RoleName = string.Join(", ", u.UserRoles.Select(ur => ur.Role.RoleName)),
                         MobileNumber = u.MobileNo ?? "N/A",
                         CreatedDate = u.CreatedAt.ToString("dd MMM yyyy"),
                         LastLogin = u.LastLoginAt.HasValue ? u.LastLoginAt.Value.ToString("dd MMM yyyy hh:mm tt") : ""
                     })
                    .FirstOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower());

                return user;
            }
            catch(Exception)
            {
                return null;
            }
        }

        //public async Task<(bool Success, string Message)> ValidateLogin(string username, string password, string roleName)
        //{
        //    // 1️⃣ Fetch user with roles
        //    var user = await _context.Users
        //        .Include(u => u.UserRol)
        //            .ThenInclude(ur => ur.Role)
        //        .FirstOrDefaultAsync(u => u.Username == username);

        //    if (user == null)
        //        return (false, "User not found");

        //    if (!user.IsActive)
        //        return (false, "Account is inactive");

        //    if (user.LockoutUntil != null && user.LockoutUntil > DateTime.Now)
        //        return (false, $"Account is locked until {user.LockoutUntil}");

        //    // 2️⃣ Verify password
        //    var hasher = new PasswordHasher<object>();
        //    var result = hasher.VerifyHashedPassword(null, user.PasswordHash, password);

        //    if (result != PasswordVerificationResult.Success)
        //    {
        //        // Increment failed login count
        //        user.FailedLoginCount++;
        //        await _context.SaveChangesAsync();
        //        return (false, "Incorrect password");
        //    }

        //    // 3️⃣ Check role
        //    bool hasRole = user.UserRoles.Any(ur => ur.Role.Name == roleName);
        //    if (!hasRole)
        //        return (false, "User does not have the required role");

        //    // 4️⃣ Reset failed login count & update last login
        //    user.FailedLoginCount = 0;
        //    user.LastLogin = DateTime.Now;
        //    await _context.SaveChangesAsync();

        //    return (true, "Login successful");
        //}

        public async Task<ChangePasswordResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            // Get user
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ChangePasswordResult
                {
                    Succeeded = false,
                    Errors = new[] { "User not found" }
                };
            }

            // Change password
            // Verify current password
            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                return new ChangePasswordResult
                {
                    Succeeded = false,
                    Errors = new[] { "Current password is incorrect" }
                };
            }

            // Hash new password
            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);

            await _context.SaveChangesAsync();

            return new ChangePasswordResult
            {
                Succeeded = true,
                Errors = new string[0]
            };

        }



    }
}
