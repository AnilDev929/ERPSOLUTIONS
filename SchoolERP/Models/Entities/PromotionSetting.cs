namespace SchoolERP.Models.Entities
{
    public class PromotionSetting
    {
        public int Id { get; set; }

        public int AcademicYearId { get; set; }

        public DateTime PromotionStartDate { get; set; }
        public DateTime PromotionEndDate { get; set; }

        public bool IsPromotionOpen { get; set; }

        public bool IsLocked { get; set; } // once done, cannot reopen

        // ✅ ADD THIS
        public AcademicYear AcademicYear { get; set; }
    }
}
