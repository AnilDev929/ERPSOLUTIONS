namespace SchoolERP.Models.DTOS
{
    public class PromotionSettingDto
    {
        public int Id { get; set; }
        public string AcademicYear { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool IsOpen { get; set; }
        public bool IsLocked { get; set; }
    }
}
