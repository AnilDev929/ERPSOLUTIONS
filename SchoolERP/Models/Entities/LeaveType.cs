namespace SchoolERP.Models.Entities
{
    public class LeaveType
    {
        public int Id { get; set; }
        public string Name { get; set; }            // Sick, Casual, etc.
        public string? Description { get; set; }
        public int MaxDaysPerYear { get; set; }
        public bool AllowCarryForward { get; set; }
        public int MaxCarryForwardDays { get; set; }
        public bool IsActive { get; set; }
        //public ICollection<LeaveRequest> LeaveRequests { get; set; } // ✅ optional
    }
}
