using FluentValidation;
using Salamtak.Shared.DTOs.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Notifications
{
    public class CreateNotificationValidator : AbstractValidator<CreateNotificationDto>
    {
        public CreateNotificationValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(150)
                .WithMessage("Title must not exceed 150 characters.");

            RuleFor(x => x.Message)
                .NotEmpty()
                .MaximumLength(1000)
                .WithMessage("Message must not exceed 1000 characters.");

            RuleFor(x => x.Type)
                .NotEmpty()
                .WithMessage("Notification type is required.");

            RuleFor(x => x.Channel)
                .NotEmpty()
                .Must(x => x == "InApp" || x == "Email" || x == "SMS")
                .WithMessage("Invalid notification channel.");
        }
    }
}
