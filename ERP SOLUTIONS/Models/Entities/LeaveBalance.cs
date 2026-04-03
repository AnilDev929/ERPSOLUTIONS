namespace ERP_SOLUTIONS.Models.Entities
{
    public class LeaveBalance
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int LeaveTypeId { get; set; }

        public int Year { get; set; }

        public int AllocatedDays { get; set; }
        public decimal UsedDays { get; set; }
        public int CarriedForwardDays { get; set; }
    }
}
