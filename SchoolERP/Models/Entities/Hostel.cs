namespace SchoolERP.Models.Entities
{
    public class Hostel
    {
        public int HostelId { get; set; }
        public string HostelName { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }

        public ICollection<Room> Rooms { get; set; }
    }

    public class Room
    {
        public int RoomId { get; set; }
        public int HostelId { get; set; }
        public string RoomNumber { get; set; }
        public int Capacity { get; set; }
        public string RoomType { get; set; }
        public decimal Fee { get; set; }
        public bool IsDeleted { get; set; } // Soft delete flag
        public int OccupiedBeds { get; set; } = 0;
    }

    public class StudentHostel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int RoomId { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime? LeaveDate { get; set; }
    }

    public class HostelFee
    {
        public int FeeId { get; set; }
        public int StudentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime FeeMonth { get; set; }
        public bool IsPaid { get; set; }
    }

    public class Payment
    {
        public int PaymentId { get; set; }
        public int StudentId { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMode { get; set; }
    }
}
