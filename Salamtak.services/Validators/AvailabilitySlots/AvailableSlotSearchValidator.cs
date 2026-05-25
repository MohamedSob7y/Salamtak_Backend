using FluentValidation;
using Salamtak.Shared.DTOs.AvailabilitySlots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.AvailabilitySlots
{
    public class AvailableSlotSearchValidator : AbstractValidator<AvailableSlotSearchDto>
    {
        public AvailableSlotSearchValidator()
        {
            RuleFor(x => x.Date)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .When(x => x.Date.HasValue)
                .WithMessage("Date cannot be in the past.");
        }
    }
}
