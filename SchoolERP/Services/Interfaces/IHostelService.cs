using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;

namespace SchoolERP.Services.Interfaces
{
    public interface IHostelService
    {
        Task<IEnumerable<HostelWithRoomsVM>> GetAllHostelsAsync();
        Task CreateHostelAsync(HostelWithRoomsVM model);
        Task<HostelWithRoomsVM> GetHostelWithRoomsAsync(int hostelId);
        Task AddHostelAsync(Hostel hostel, List<RoomVM> rooms);
        Task UpdateHostelAsync(HostelWithRoomsVM model);
        Task DeleteRoomAsync(int roomId);
    }
}
