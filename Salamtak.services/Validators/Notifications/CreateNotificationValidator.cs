using FluentValidation;
using Salamtak.Domain.Models.Enums;
using Salamtak.Shared.DTOs.Notifications;

namespace Salamtak.services.Validators.Notifications
{
    public class CreateNotificationValidator
        : AbstractValidator<CreateNotificationDto>
    {
        public CreateNotificationValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(x => x.AppointmentId)
                .Must(appointmentId =>
                    !appointmentId.HasValue ||
                    appointmentId.Value != Guid.Empty)
                .WithMessage("AppointmentId is invalid.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required.")
                .MaximumLength(150)
                .WithMessage(
                    "Title must not exceed 150 characters.");

            RuleFor(x => x.Message)
                .NotEmpty()
                .WithMessage("Message is required.")
                .MaximumLength(1000)
                .WithMessage(
                    "Message must not exceed 1000 characters.");

            RuleFor(x => x.Type)
                .NotEmpty()
                .WithMessage("Notification type is required.")
                .Must(type =>
                    Enum.TryParse<NotificationType>(
                        type,
                        true,
                        out _))
                .WithMessage("Invalid notification type.");
        }
    }
}