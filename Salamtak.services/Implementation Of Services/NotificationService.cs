using AutoMapper;
using External_Services.Email;
using FluentValidation;
using Microsoft.Extensions.Logging;
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

        private readonly IValidator<CreateNotificationDto>
            _createValidator;

        private readonly IValidator<MarkNotificationAsReadDto>
            _markValidator;

        private readonly IEmailService _emailService;

        private readonly IRealtimeNotificationService
            _realtimeNotificationService;

        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateNotificationDto> createValidator,
            IValidator<MarkNotificationAsReadDto> markValidator,
            IEmailService emailService,
            IRealtimeNotificationService realtimeNotificationService,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _markValidator = markValidator;
            _emailService = emailService;

            _realtimeNotificationService =
                realtimeNotificationService;

            _logger = logger;
        }

        public async Task<ApiResponse<NotificationDto>>
            CreateAsync(CreateNotificationDto dto)
        {
            var validationResult =
                await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors.Select(
                        error => error.ErrorMessage));
            }

            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(dto.UserId);

            if (user is null)
            {
                throw new NotFoundException(
                    "User not found.");
            }

            if (dto.AppointmentId.HasValue)
            {
                var appointmentExists =
                    await _unitOfWork
                        .Repository<Appointment>()
                        .AnyAsync(appointment =>
                            appointment.Id ==
                            dto.AppointmentId.Value);

                if (!appointmentExists)
                {
                    throw new NotFoundException(
                        "Appointment not found.");
                }
            }

            if (!Enum.TryParse<NotificationType>(
                    dto.Type,
                    true,
                    out var type))
            {
                throw new BadRequestException(
                    "Invalid notification type.");
            }

            /*
             * كل Notification في المشروع
             * لازم تتبعت:
             *
             * 1- In App باستخدام SignalR
             * 2- Email
             */
            var channel =
                NotificationChannel.InAppAndEmail;

            var notification =
                new Notification
                {
                    UserId = user.Id,

                    AppointmentId =
                        dto.AppointmentId,

                    Title =
                        dto.Title.Trim(),

                    Message =
                        dto.Message.Trim(),

                    Type = type,

                    Channel = channel,

                    Status =
                        NotificationStatus.Pending,

                    IsRead = false,

                    SentAt = null
                };

            /*
             * نحفظ Notification في Database الأول.
             */
            await _unitOfWork
                .Repository<Notification>()
                .AddAsync(notification);

            await _unitOfWork.SaveChangesAsync();

            var inAppSucceeded = false;
            var emailSucceeded = false;

            /*
             * ==============================
             * SignalR / In-App Notification
             * ==============================
             */
            try
            {
                var realtimeDto =
                    _mapper.Map<RealtimeNotificationDto>(
                        notification);

                await _realtimeNotificationService
                    .SendToUserAsync(
                        user.Id,
                        realtimeDto);

                inAppSucceeded = true;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "SignalR notification failed for user {UserId}.",
                    user.Id);
            }

            /*
             * ==============================
             * Email Notification
             * ==============================
             */
            try
            {
                if (string.IsNullOrWhiteSpace(
                        user.Email))
                {
                    _logger.LogWarning(
                        "Email notification could not be sent because user {UserId} has no email.",
                        user.Id);
                }
                else
                {
                    await _emailService
                        .SendEmailAsync(
                            user.Email,
                            notification.Title,
                            notification.Message);

                    emailSucceeded = true;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Email notification failed for user {UserId}.",
                    user.Id);
            }

            /*
             * الـNotification تعتبر Sent
             * لو قناة واحدة على الأقل نجحت.
             *
             * لو الاتنين فشلوا تبقى Failed.
             */
            var anyDeliverySucceeded =
                inAppSucceeded ||
                emailSucceeded;

            notification.Status =
                anyDeliverySucceeded
                    ? NotificationStatus.Sent
                    : NotificationStatus.Failed;

            notification.SentAt =
                anyDeliverySucceeded
                    ? DateTime.UtcNow
                    : null;

            _unitOfWork
                .Repository<Notification>()
                .Update(notification);

            await _unitOfWork.SaveChangesAsync();

            var result =
                MapNotification(
                    notification,
                    user);

            string message;

            if (inAppSucceeded &&
                emailSucceeded)
            {
                message =
                    "Notification was delivered successfully in-app and by email.";
            }
            else if (inAppSucceeded)
            {
                message =
                    "Notification was delivered in-app, but email delivery failed.";
            }
            else if (emailSucceeded)
            {
                message =
                    "Notification was delivered by email, but real-time in-app delivery failed.";
            }
            else
            {
                message =
                    "Notification was saved, but all delivery channels failed.";
            }

            return ApiResponse<NotificationDto>.Ok(
                result,
                message);
        }

        public async Task<
            ApiResponse<
                IReadOnlyList<NotificationDto>>>
            GetUserNotificationsAsync(
                Guid userId)
        {
            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(userId);

            if (user is null)
            {
                throw new NotFoundException(
                    "User not found.");
            }

            var notifications =
                await _unitOfWork
                    .Repository<Notification>()
                    .GetAllAsync(notification =>
                        notification.UserId ==
                        userId &&
                        !notification.IsDeleted);

            var result = notifications
                .OrderByDescending(
                    notification =>
                        notification.CreatedAt)
                .Select(notification =>
                    MapNotification(
                        notification,
                        user))
                .ToList();

            return ApiResponse<
                IReadOnlyList<
                    NotificationDto>>.Ok(
                        result,
                        "User notifications retrieved successfully.");
        }

        public async Task<
            ApiResponse<
                IReadOnlyList<NotificationDto>>>
            GetAllNotificationsAsync()
        {
            var notifications =
                await _unitOfWork
                    .Repository<Notification>()
                    .GetAllAsync(notification =>
                        !notification.IsDeleted);

            var orderedNotifications =
                notifications
                    .OrderByDescending(
                        notification =>
                            notification.CreatedAt)
                    .ToList();

            var userIds =
                orderedNotifications
                    .Select(notification =>
                        notification.UserId)
                    .Distinct()
                    .ToList();

            var users = await _unitOfWork
                .Repository<User>()
                .GetAllAsync(user =>
                    userIds.Contains(user.Id));

            var userLookup =
                users.ToDictionary(
                    user => user.Id,
                    user => user);

            var result =
                orderedNotifications
                    .Select(notification =>
                    {
                        userLookup.TryGetValue(
                            notification.UserId,
                            out var user);

                        return MapNotification(
                            notification,
                            user);
                    })
                    .ToList();

            return ApiResponse<
                IReadOnlyList<
                    NotificationDto>>.Ok(
                        result,
                        "All notifications retrieved successfully.");
        }

        public async Task<ApiResponse>
            MarkAsReadAsync(
                Guid userId,
                MarkNotificationAsReadDto dto)
        {
            var validationResult =
                await _markValidator
                    .ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors.Select(
                        error =>
                            error.ErrorMessage));
            }

            var notification =
                await _unitOfWork
                    .Repository<Notification>()
                    .GetByIdAsync(
                        dto.NotificationId);

            if (notification is null ||
                notification.IsDeleted)
            {
                throw new NotFoundException(
                    "Notification not found.");
            }

            if (notification.UserId !=
                userId)
            {
                throw new ForbiddenException(
                    "You are not allowed to update this notification.");
            }

            if (notification.IsRead)
            {
                return ApiResponse.Ok(
                    "Notification is already marked as read.");
            }

            notification.IsRead = true;

            _unitOfWork
                .Repository<Notification>()
                .Update(notification);

            await _unitOfWork
                .SaveChangesAsync();

            return ApiResponse.Ok(
                "Notification marked as read.");
        }

        public async Task<ApiResponse>
            MarkAllAsReadAsync(
                Guid userId)
        {
            var userExists =
                await _unitOfWork
                    .Repository<User>()
                    .AnyAsync(user =>
                        user.Id ==
                        userId);

            if (!userExists)
            {
                throw new NotFoundException(
                    "User not found.");
            }

            var notifications =
                await _unitOfWork
                    .Repository<Notification>()
                    .GetAllAsync(
                        notification =>
                            notification.UserId ==
                            userId &&
                            !notification.IsRead &&
                            !notification.IsDeleted);

            foreach (
                var notification
                in notifications)
            {
                notification.IsRead =
                    true;

                _unitOfWork
                    .Repository<Notification>()
                    .Update(notification);
            }

            await _unitOfWork
                .SaveChangesAsync();

            return ApiResponse.Ok(
                "All notifications marked as read.");
        }

        private NotificationDto
            MapNotification(
                Notification notification,
                User? user)
        {
            var result =
                _mapper.Map<NotificationDto>(
                    notification);

            result.RecipientName =
                user?.FullName ??
                string.Empty;

            result.RecipientEmail =
                user?.Email ??
                string.Empty;

            result.RecipientRole =
                user?.Role.ToString() ??
                string.Empty;

            return result;
        }
    }
}