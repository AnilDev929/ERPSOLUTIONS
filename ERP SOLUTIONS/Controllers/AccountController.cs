
using ERP_SOLUTIONS.Models.DTOS;
using ERP_SOLUTIONS.Models.ViewModels;
using ERP_SOLUTIONS.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERP_SOLUTIONS.Controllers
{
    [AllowAnonymous]  // Make login accessible to all
    public class AccountController :Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IAccountService _accountService;

        public AccountController(IHttpClientFactory clientFactory, IAccountService accountService)
        {
            _clientFactory = clientFactory;
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var roles = _accountService.GetRolesDropdown();

            var model = new LoginViewModel
            {
                Roles = roles,
                SelectedRoleID = roles.Count == 1 ? int.Parse(roles.First().Value) : 0
            };

            return View(model);
        }

        // POST: Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            try
            {
                if (string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
                {
                    TempData["Error"] = "Username and Password are required";
                    login.Roles = _accountService.GetRolesDropdown();
                    return View();
                }

                var result = await _accountService.LoginAsync(login.Username, login.Password, login.SelectedRoleID);

                if (result.Success)
                {
                    //Get the user detail
                    UserInfoDTO userInfo = await _accountService.GetUserInfoAsync(login.Username);

                    // Create claims
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userInfo.UserID.ToString()),
                    new Claim(ClaimTypes.Name, userInfo.UserName),
                    new Claim("FullName", userInfo.FullName),
                    new Claim(ClaimTypes.Email, userInfo.Email),
                    new Claim(ClaimTypes.Role, userInfo.RoleName),
                    new Claim("RoleID", userInfo.RoleID.ToString()),
                    new Claim("MobileNumber", userInfo.MobileNumber),
                    new Claim("CreatedOn", userInfo.CreatedDate),
                    new Claim("LastLogin", userInfo.LastLogin)
                };

                    // Create identity and principal
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // Sign in user (await is important!)
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // Redirect to Dashboard
                    return RedirectToAction("Index", "Dashboard");

                    ////Access Claims Anywhere
                    //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    //var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                    //var fullName = User.FindFirst("FullName")?.Value;
                    //var email = User.FindFirst(ClaimTypes.Email)?.Value;
                    //var role = User.FindFirst(ClaimTypes.Role)?.Value;
                    //var mobile = User.FindFirst("MobileNumber")?.Value;
                }
                else
                {
                    TempData["Error"] = result.Message;
                    login.Roles = _accountService.GetRolesDropdown();
                    return View(login);
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong, try again!";
                login.Roles = _accountService.GetRolesDropdown();
                return View();
            }
        }


        public IActionResult MyProfile()
        {
            var roleName = User.FindFirst(ClaimTypes.Role)?.Value;

            switch (roleName.ToLower())
            {
                case "admin":
                    // logic for Admin
                    return RedirectToAction("Profile", "Admin");

                case "superadmin":
                    // logic for SuperAdmin
                    return RedirectToAction("Profile", "SuperAdmin");

                case "teacher":
                    // logic for Teacher
                    return RedirectToAction("Profile", "Teacher");

                case "student":
                    // logic for Student
                    return RedirectToAction("MyProfile", "Students");

                case "accountant":
                    // logic for Accountant
                    return RedirectToAction("Profile", "Accountant");

                default:
                    // role not recognized
                    return RedirectToAction("AccessDenied", "Account");

            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _accountService.ChangePasswordAsync(Convert.ToInt32(userId), model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = "Your password has been changed successfully.";
                return RedirectToAction("ChangePassword");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            return View(model);
        }

        public IActionResult Error()
        {
            return View();
        }

        // GET: Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
