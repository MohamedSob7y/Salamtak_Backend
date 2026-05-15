using FluentValidation;
using Salamtak.Shared.DTOs.AvailabilitySlots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.AvailabilitySlots
{
    public class CreateAvailabilitySlotValidator : AbstractValidator<CreateAvailabilitySlotDto>
    {
        public CreateAvailabilitySlotValidator()
        {
            RuleFor(x => x.DoctorId).NotEmpty();

            RuleFor(x => x.ClinicId).NotEmpty();

            RuleFor(x => x.StartTime)
                .NotEmpty()
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Start time must be in the future.");

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .GreaterThan(x => x.StartTime)
                .WithMessage("End time must be after start time.");
        }
    }
}
