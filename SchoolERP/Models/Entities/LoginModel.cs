using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.Entities
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email or Mobile is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }
    }
}
