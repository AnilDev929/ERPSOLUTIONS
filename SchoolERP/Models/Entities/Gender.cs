using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.Entities
{
    public class Gender
    {
        [Key]
        public int GenderID { get; set; }

        [Required]
        public string GenderName { get; set; }
    }
}
