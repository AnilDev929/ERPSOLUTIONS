namespace SchoolERP.Models.ViewModels
{
    public class RoomVM
    {
        public int? RoomId { get; set; } // NULL = New Room
        public string RoomNumber { get; set; }
        public int Capacity { get; set; }
        public string RoomType { get; set; }
        public decimal Fee { get; set; }
        public int OccupiedBeds { get; set; } = 0;
        public bool IsDeleted { get; set; } = false; // Soft delete flag
    }

    public class RoomEditVM : RoomVM
    {
        public int? RoomId { get; set; }
        public bool IsDeleted { get; set; } = false; // Soft delete flag

        public string Status
        {
            get
            {
                if (OccupiedBeds == 0) return "Empty";
                if (OccupiedBeds < Capacity) return "Available";
                return "Full";
            }
        }
    }
}
