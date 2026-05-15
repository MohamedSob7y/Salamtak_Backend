using FluentValidation;
using Salamtak.Shared.DTOs.AvailabilitySlots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.AvailabilitySlots
{
    public class UpdateAvailabilitySlotValidator : AbstractValidator<UpdateAvailabilitySlotDto>
    {
        public UpdateAvailabilitySlotValidator()
        {
            RuleFor(x => x.SlotId).NotEmpty();

            RuleFor(x => x.StartTime).NotEmpty();

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .GreaterThan(x => x.StartTime)
                .WithMessage("End time must be after start time.");
        }
    }
}
