namespace ERP_SOLUTIONS.Models.DTOS
{
    public class StudentProfileDto
    {
        // 🔹 Basic Info
        public string Name { get; set; }
        public string RollNumber { get; set; }
        public int Gender { set; get; }
        public string Email { get; set; }

        // 🔹 Personal Info
        public string Phone { get; set; }
        public string Address { get; set; }

        // 🔹 Academic Info
        public string Class { get; set; }
        public string Section { get; set; }
        public int Year { get; set; }

        // 🔹 Login Info
        public string UserName { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? PreviousLoginAt { get; set; }

        // 🔹 UI Extras
        public int ProfileCompletion { get; set; }
        public bool IsActive { get; set; }
    }
}
