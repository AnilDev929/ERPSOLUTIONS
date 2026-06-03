using SchoolERP.Models.DTOS;

namespace SchoolERP.Models.ViewModels
{
    public class MenuSectionVM
    {
        // Section fields
        public int Id { set; get; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; } = "#4e73df";
        public string Subtitle { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; } = 0;
        // Items list
        public List<MenuItemDTO> Items { get; set; } = new();

        // Existing items to display
        public List<MenuItemDTO> ExistingItems { get; set; } = new();
    }
}
