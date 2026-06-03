
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Models.Entities
{
    [Table("MenuItems")]
    public class MenuItem
    {
        public int Id { get; set; }
        public int SectionId { get; set; }

        [ForeignKey("SectionId")] // 🔥 important
        public MenuSection MenuSection { get; set; }

        public string Title { get; set; }
        public string? Description { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }


}
