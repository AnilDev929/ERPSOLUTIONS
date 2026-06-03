namespace SchoolERP.Models.Entities
{
    public class MenuSetting
    {
        public int Id { get; set; }

        public string MenuKey { get; set; }

        public string MenuName { get; set; }

        public bool IsActive { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
