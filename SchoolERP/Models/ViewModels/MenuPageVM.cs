using SchoolERP.Models.DTOS;

namespace SchoolERP.Models.ViewModels
{
    public class MenuPageVM
    {
        public MenuSectionVM NewSection { get; set; } = new MenuSectionVM();

        public List<MenuSectionVM>? ExistingSections { get; set; } = new();

        public List<MenuItemDTO> NewItems { get; set; } = new List<MenuItemDTO>(); // 👈 separate from section
    }
}
