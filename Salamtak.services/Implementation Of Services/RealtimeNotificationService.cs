using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Notifications;

namespace Salamtak.services.Implementation_Of_Services
{
    public class RealtimeNotificationService : IRealtimeNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RealtimeNotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task SendToUserAsync(Guid userId, RealtimeNotificationDto dto)
        {
            return SaveNotificationAsync(userId, dto);
        }

        public Task SendToDoctorAsync(Guid doctorUserId, RealtimeNotificationDto dto)
        {
            return SaveNotificationAsync(doctorUserId, dto);
        }

        public Task SendToPatientAsync(Guid patientUserId, RealtimeNotificationDto dto)
        {
            return SaveNotificationAsync(patientUserId, dto);
        }

        public Task SendToAdminAsync(Guid adminUserId, RealtimeNotificationDto dto)
        {
            return SaveNotificationAsync(adminUserId, dto);
        }

        private async Task SaveNotificationAsync(Guid userId, RealtimeNotificationDto dto)
        {
            var exists = await _unitOfWork.Repository<User>().AnyAsync(u => u.Id == userId);
            if (!exists)
                return;

            var notification = new Notification
            {
                UserId = userId,
                Title = dto.Title.Trim(),
                Message = dto.Message.Trim(),
                Type = Enum.TryParse<NotificationType>(dto.Type, true, out var type) ? type : default,
                Channel = NotificationChannel.InApp,
                Status = NotificationStatus.Sent,
                IsRead = false,
                SentAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Notification>().AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();
        }
    }//مهوش لازمة لازم يتسمح
}
