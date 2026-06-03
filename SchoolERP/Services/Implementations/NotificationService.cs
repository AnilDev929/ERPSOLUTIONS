using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Common;
using SchoolERP.Data;
using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;
using SchoolERP.Models.ViewModels;
using SchoolERP.Services.Interfaces;

namespace SchoolERP.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetNotifications()
        {
            var data = await _context.Notifications
               .Where(x => x.IsActive)
               .OrderByDescending(x => x.CreatedAt)
               .ToListAsync();

            return data;
        }

        #region CREATE / UPDATE

        public async Task<ServiceResponse> CreateOrUpdateAsync(NotificationVM model, int userId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 🔴 DUPLICATE CHECK
                var isDuplicate = await IsDuplicateAsync(model.Title, model.StartDate.Value, model.NotificationId);
                if (isDuplicate)
                    return ServiceResponse.Fail("Duplicate notification exists");

                Notification entity;

                if (model.NotificationId > 0)
                {
                    // UPDATE
                    entity = await _context.Notifications
                        .Include(x => x.Targets)
                        .FirstOrDefaultAsync(x => x.NotificationId == model.NotificationId);

                    if (entity == null)
                        return ServiceResponse.Fail("Notification not found");

                    entity.Title = model.Title;
                    entity.Message = model.Message;
                    entity.Priority = model.Priority;
                    entity.StartDate = model.StartDate.Value;
                    entity.EndDate = model.EndDate.Value;

                    // remove old roles
                    _context.NotificationTargets.RemoveRange(entity.Targets);
                }
                else
                {
                    // INSERT
                    entity = new Notification
                    {
                        Title = model.Title,
                        Message = model.Message,
                        Priority = model.Priority,
                        StartDate = model.StartDate.Value,
                        EndDate = model.EndDate.Value,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId,
                        IsActive = true
                    };

                    _context.Notifications.Add(entity);
                }

                // 🔥 Save once to get NotificationId (for insert)
                await _context.SaveChangesAsync();

                // roles mapping
                foreach (var roleId in model.RoleIds)
                {
                    _context.NotificationTargets.Add(new NotificationTarget
                    {
                        NotificationId = entity.NotificationId,
                        RoleId = roleId
                    });
                }

                await _context.SaveChangesAsync();

                // ✅ Commit
                await transaction.CommitAsync();

                return ServiceResponse.Ok("Saved successfully");
            }
            catch (Exception ex)
            {
                // ❌ Rollback on error
                await transaction.RollbackAsync();

                // TODO: log ex (important in real apps)
                return ServiceResponse.Fail("Something went wrong while saving notification");
            }
        }

        #endregion

        #region DUPLICATE CHECK (UPDATE SAFE)

        public async Task<bool> IsDuplicateAsync(string title, DateTime startTime, int? id = null)
        {
            return await _context.Notifications.AnyAsync(x =>
                x.Title == title &&
                x.StartDate == startTime &&
                (!id.HasValue || x.NotificationId != id));
        }

        #endregion

        #region SOFT DELETE

        public async Task<ServiceResponse> SoftDeleteAsync(int id)
        {
            var entity = await _context.Notifications.FindAsync(id);

            if (entity == null)
                return ServiceResponse.Fail("Not found");

            entity.IsActive = false;

            await _context.SaveChangesAsync();

            return ServiceResponse.Ok("Deleted successfully");
        }

        #endregion

        #region ACTIVE NOTIFICATIONS

        public async Task<List<NotificationDTO>> GetActiveForUserAsync(int userId, int roleId)
        {
            //var now = DateTime.Today;
            //return await (from n in _context.Notifications
            //              join t in _context.NotificationTargets
            //              on n.NotificationId equals t.NotificationId
            //              join s in _context.NotificationReadStatus
            //              on n.NotificationId equals s.NotificationId
            //              where t.RoleId == roleId
            //                    && n.IsActive
            //                    && n.StartDate <= now
            //                    && n.EndDate >= now
            //              orderby n.Priority descending
            //              select n).ToListAsync();

            var today = DateTime.Today;

            var data = await (from n in _context.Notifications
                              join t in _context.NotificationTargets
                                  on n.NotificationId equals t.NotificationId
                              join r in _context.NotificationReadStatus
                                  on new { n.NotificationId, UserId = userId }
                                  equals new { r.NotificationId, r.UserId }
                                  into readGroup
                              from r in readGroup.DefaultIfEmpty()

                              where t.RoleId == roleId
                                    && n.IsActive
                                    && n.EndDate >= today   // 🔥 KEY FIX

                              orderby n.StartDate ascending, n.Priority descending

                              select new NotificationDTO
                              {
                                  NotificationId = n.NotificationId,
                                  Title = n.Title,
                                  Message = n.Message,
                                  StartDate = n.StartDate,
                                  EndDate = n.EndDate,
                                  Priority = n.Priority,
                                  IsRead = r != null && r.IsRead
                              })
                              .Distinct()
                              .ToListAsync();

            return data;
        }

        #endregion

        #region MARK AS READ

        public async Task<ServiceResponse> MarkAsReadAsync(int notificationId, int userId)
        {
            //var exists = await _context.NotificationReadStatus
            //    .AnyAsync(x => x.NotificationId == notificationId && x.UserId == userId);

            var entity = await _context.NotificationReadStatus
                .FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.UserId == userId);

            //if (!exists)
            if (entity == null)
            {
                _context.NotificationReadStatus.Add(new NotificationReadStatus
                {
                    NotificationId = notificationId,
                    UserId = userId,
                    ReadAt = DateTime.Now
                });
            }
            else
            {
                // UPDATE
                entity.IsRead = true;
                entity.ReadAt = DateTime.Now;
            }
            await _context.SaveChangesAsync();

            return ServiceResponse.Ok("Marked as read");
        }

        #endregion

        #region EXPIRE

        public async Task ExpireOldNotificationsAsync()
        {
            var now = DateTime.Now;

            var list = await _context.Notifications
                .Where(x => x.EndDate < now && x.IsActive)
                .ToListAsync();

            foreach (var item in list)
                item.IsActive = false;

            await _context.SaveChangesAsync();
        }

        #endregion

        #region EDIT DATA

        public async Task<NotificationVM> GetByIdForEditAsync(int id)
        {
            // Always load roles first
            var roles = await _context.Roles
                 .Where(r => r.IsActive)   // ✅ filter first
                .Select(r => new SelectListItem
                {
                    Value = r.RoleID.ToString(),
                    Text = r.RoleName
                }).ToListAsync();

            var data = await _context.Notifications
                .Include(x => x.Targets)
                .FirstOrDefaultAsync(x => x.NotificationId == id);

            // If NOT found → return empty model with roles
            if (data == null)
            {
                return new NotificationVM
                {
                    Roles = roles,
                    RoleIds = new List<int>() // avoid null issues in view
                };
            }

            // If found → map data
            return new NotificationVM
            {
                NotificationId = data.NotificationId,
                Title = data.Title,
                Message = data.Message,
                Priority = data.Priority,
                StartDate = data.StartDate,
                EndDate = data.EndDate,
                RoleIds = data.Targets.Select(x => x.RoleId).ToList(),
                Roles = roles
            };
        }

        #endregion
    }
}
