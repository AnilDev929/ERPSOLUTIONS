using SchoolERP.Models.DTOS;

namespace SchoolERP.Models.ViewModels
{
    public class ParentProfileViewModel
    {
        public int ParentId { get; set; }
        public string ParentName { get; set; }

        public string Email { get; set; }

        public string MobileNumber { get; set; }

        public string Address { get; set; }

        public List<StudentDto> Students { get; set; }
    }
}
