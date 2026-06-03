namespace SchoolERP.Models.Entities
{
    public class PromotionState
    {
        public bool IsOpen { get; set; }
        public bool IsLocked { get; set; }
        public bool IsWithinDateRange { get; set; }

        public bool CanAccess => IsOpen && IsWithinDateRange && !IsLocked;
    }
}
