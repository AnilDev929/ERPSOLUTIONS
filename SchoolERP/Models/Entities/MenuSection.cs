using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Models.Entities
{
    [Table("MenuSections")]
    public class MenuSection
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public string? Subtitle { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; } = 0;
        //Avoid null reference issues
        public string? ComponentName { get; set; }  // e.g. "PromotionMenu"
        public bool? IsComponent { get; set; }
        public List<MenuItem> Items { get; set; } = new();
    }
}
