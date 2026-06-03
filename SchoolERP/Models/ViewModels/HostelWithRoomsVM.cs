using SchoolERP.Models.Entities;

namespace SchoolERP.Models.ViewModels
{
    public class HostelWithRoomsVM
    {
        public Hostel Hostel { get; set; }
        public List<RoomVM> Rooms { get; set; }
    }
}
