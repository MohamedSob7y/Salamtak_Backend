using FluentValidation;
using Salamtak.Shared.DTOs.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Notifications
{
    public class MarkNotificationAsReadValidator : AbstractValidator<MarkNotificationAsReadDto>
    {
        public MarkNotificationAsReadValidator()
        {
            RuleFor(x => x.NotificationId)
                .NotEmpty();
        }
    }
}
