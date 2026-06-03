using SchoolERP.Common;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;

namespace SchoolERP.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ServiceResponse> CreateOrUpdateAsync(NotificationVM model, int userId);

        Task<List<NotificationDTO>> GetActiveForUserAsync(int userId, int roleId);

        Task<ServiceResponse> MarkAsReadAsync(int notificationId, int userId);

        Task<bool> IsDuplicateAsync(string title, DateTime startTime, int? id = null);

        Task ExpireOldNotificationsAsync();

        Task<ServiceResponse> SoftDeleteAsync(int id);
        Task<NotificationVM> GetByIdForEditAsync(int id);

        Task<List<Notification>> GetNotifications();
    }
}
