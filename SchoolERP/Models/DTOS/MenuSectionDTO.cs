
namespace SchoolERP.Models.DTOS
{
    public class MenuSectionDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public bool IsActive { get; set; }
        public string Subtitle { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public string? ComponentName { get; set; }  // e.g. "PromotionMenu"
        public bool? IsComponent { get; set; } = false;
        public List<MenuItemDTO> Items { get; set; }
    }

    public class MenuItemDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Subtitle { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; }
        public string Url { get; set; }
    }

}
