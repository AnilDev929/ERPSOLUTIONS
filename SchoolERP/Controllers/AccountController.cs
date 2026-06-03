
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Common;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SchoolERP.Controllers
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
                    return RedirectToAction("Login"); // ✅ FIX
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
                    return RedirectToAction("Login");
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong, try again!";
                login.Roles = _accountService.GetRolesDropdown();
                return RedirectToAction("Login"); // ✅ FIX
            }
        }


        public IActionResult ParentLogIn()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ParentLogIn(LoginModel model)
        {
            try
            {
                // Check model validation
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fill all required fields.";
                    return View(model);
                }

                // Validate Email or Mobile
                bool isEmail = Regex.IsMatch(model.UserName,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                bool isMobile = Regex.IsMatch(model.UserName,
                    @"^[6-9]\d{9}$");

                if (!isEmail && !isMobile)
                {
                    TempData["Error"] = "Enter valid Email or Mobile number.";
                    return View(model);
                }

                var status = await _accountService.ParentLogin(model);
                //------------------------------------
                // Success
                //------------------------------------
                if (status.Success == 1)
                {
                    int parentid = Convert.ToInt32(status.Message);
                    User user = await _accountService.GetParentDtl(parentid);

                    // Create claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim("FullName", user.FullName ?? ""),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, "Parent"),
                        new Claim("MobileNumber", user.MobileNo ?? ""),
                        new Claim("LastLoginAt",
                        user.LastLoginAt.Value.ToString("dd MMM yyyy hh mm tt"))
                    };

                    // Create identity and principal
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // Sign in user (await is important!)
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToAction("Parents", "Dashboard");
                }

                //------------------------------------
                // Failure
                //------------------------------------
                TempData["Error"] = status.Message;
                return RedirectToAction("ParentLogIn", "Account");
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong. Please try again.";
                return View(model);
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
                case "parent":
                    // logic for Accountant
                    return RedirectToAction("Profile", "Parent");
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

        public IActionResult AccessDenied()
        {
            return View();

        }
        public IActionResult Error()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json("Email is required");
            }

            if (!Regex.IsMatch(email,
                @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            {
                return Json("Invalid email address");
            }

            return Json(true);
        }
        /*
         Indian Mobile Number Validation
            Indian mobile numbers:
            must be 10 digits
            usually start with: (6 7 8 9)
         */
        [HttpPost]
        public JsonResult ValidatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Json("Phone number is required");
            }
            // Indian mobile number validation
            if (!Regex.IsMatch(phoneNumber, @"^[6-9]\d{9}$"))
            {
                return Json("Invalid Indian mobile number");
            }
            return Json(true);
        }

        [HttpPost]
        public IActionResult ValidateAadhar(string AadhaarNumber)
        {
            if (!AadhaarValidator.IsValid(AadhaarNumber.Trim()))
            {
                return Json("Please enter a valid Aadhaar number");
            }
            return Json(true);
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var roleName = User.FindFirst(ClaimTypes.Role)?.Value;

            await HttpContext.SignOutAsync(); // IMPORTANT

            HttpContext.Session.Clear();
            TempData.Clear();

            if (roleName.ToLower() == "parent")
            {
                return RedirectToAction("ParentLogIn", "Account");
            }
            else
                return RedirectToAction("Login", "Account");
        }
    }
}
