using Microsoft.AspNetCore.Mvc.Rendering;

namespace SchoolERP.Models.ViewModels
{
    public class LoginViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public int SelectedRoleID { get; set; }   // selected value

        public List<SelectListItem> Roles { get; set; }  // dropdown list
    }
}
