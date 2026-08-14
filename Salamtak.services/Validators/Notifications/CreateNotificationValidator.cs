using FluentValidation;
using Salamtak.Domain.Models.Enums;
using Salamtak.Shared.DTOs.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            RuleFor(x => x.Channel)
                .NotEmpty()
                .WithMessage("Notification channel is required.")
                .Must(channel =>
                {
                    if (!Enum.TryParse<NotificationChannel>(
                            channel,
                            true,
                            out var parsedChannel))
                    {
                        return false;
                    }

                    return parsedChannel is
                        NotificationChannel.InApp or
                        NotificationChannel.Email or
                        NotificationChannel.InAppAndEmail;
                })
                .WithMessage(
                    "Channel must be InApp, Email, or InAppAndEmail.");
        }
    }
}
