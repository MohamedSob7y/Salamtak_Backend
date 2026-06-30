using AutoMapper;
using External_Services.Email;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Notifications;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Implementation_Of_Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateNotificationDto> _createValidator;
        private readonly IValidator<MarkNotificationAsReadDto> _markValidator;
        private readonly IEmailService _emailService;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateNotificationDto> createValidator,
            IValidator<MarkNotificationAsReadDto> markValidator,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _markValidator = markValidator;
            _emailService = emailService;
        }

        public async Task<ApiResponse<NotificationDto>> CreateAsync(CreateNotificationDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(dto.UserId);

            if (user is null)
                throw new NotFoundException("User not found.");

            if (!Enum.TryParse<NotificationType>(dto.Type, true, out var type))
                throw new BadRequestException("Invalid notification type.");

            if (!Enum.TryParse<NotificationChannel>(dto.Channel, true, out var channel))
                throw new BadRequestException("Invalid notification channel.");

            var notification = new Notification
            {
                UserId = dto.UserId,
                AppointmentId = dto.AppointmentId,
                Title = dto.Title.Trim(),
                Message = dto.Message.Trim(),
                Type = type,
                Channel = channel,
                Status = NotificationStatus.Sent,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            await _unitOfWork
                .Repository<Notification>()
                .AddAsync(notification);

            await _unitOfWork.SaveChangesAsync();

            // ابعت Email كمان في نفس الوقت (InApp + Email)
            try
            {
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    await _emailService.SendEmailAsync(
                        user.Email,
                        dto.Title.Trim(),
                        dto.Message.Trim());
                }
            }
            catch
            {
                // لو الإيميل فشل، الـ Notification يفضل متسجل في الـ DB عادي
            }

            var result = _mapper.Map<NotificationDto>(notification);

            return ApiResponse<NotificationDto>.Ok(result, "Notification created successfully.");
        }

        public async Task<ApiResponse<IReadOnlyList<NotificationDto>>> GetUserNotificationsAsync(Guid userId)
        {
            var userExists = await _unitOfWork
                .Repository<User>()
                .AnyAsync(u => u.Id == userId);

            if (!userExists)
                throw new NotFoundException("User not found.");

            var notifications = await _unitOfWork
                .Repository<Notification>()
                .GetAllAsync(n => n.UserId == userId);

            var orderedNotifications = notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            var result = _mapper.Map<IReadOnlyList<NotificationDto>>(orderedNotifications);

            return ApiResponse<IReadOnlyList<NotificationDto>>.Ok(result);
        }

        public async Task<ApiResponse> MarkAsReadAsync(Guid userId, MarkNotificationAsReadDto dto)
        {
            var validationResult = await _markValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var notification = await _unitOfWork
                .Repository<Notification>()
                .GetByIdAsync(dto.NotificationId);

            if (notification is null)
                throw new NotFoundException("Notification not found.");

            if (notification.UserId != userId)
                throw new ForbiddenException("You are not allowed to update this notification.");

            notification.IsRead = true;

            _unitOfWork.Repository<Notification>().Update(notification);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Notification marked as read.");
        }

        public async Task<ApiResponse> MarkAllAsReadAsync(Guid userId)
        {
            var userExists = await _unitOfWork
                .Repository<User>()
                .AnyAsync(u => u.Id == userId);

            if (!userExists)
                throw new NotFoundException("User not found.");

            var notifications = await _unitOfWork
                .Repository<Notification>()
                .GetAllAsync(n => n.UserId == userId && !n.IsRead);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                _unitOfWork.Repository<Notification>().Update(notification);
            }

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("All notifications marked as read.");
        }
    }
}