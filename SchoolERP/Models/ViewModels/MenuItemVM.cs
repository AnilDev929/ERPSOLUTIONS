namespace SchoolERP.Models.ViewModels
{
    public class MenuItemVM
    {
        public int Id { get; set; } 
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; } = 0;
    }
}
