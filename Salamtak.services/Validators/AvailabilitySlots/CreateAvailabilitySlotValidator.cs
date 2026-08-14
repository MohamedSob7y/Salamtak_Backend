using FluentValidation;
using Salamtak.Shared.DTOs.AvailabilitySlots;

namespace Salamtak.services
    .Validators
    .AvailabilitySlots
{
    public class CreateAvailabilitySlotValidator
        : AbstractValidator<CreateAvailabilitySlotDto>
    {
        public CreateAvailabilitySlotValidator()
        {
            RuleFor(dto => dto.ClinicId)
                .NotEmpty()
                .WithMessage(
                    "Clinic id is required.");

            RuleFor(dto => dto.StartTime)
                .NotEmpty()
                .WithMessage(
                    "Start time is required.")
                .Must(startTime =>
                    startTime > DateTime.UtcNow)
                .WithMessage(
                    "Start time must be in the future.");

            RuleFor(dto => dto.EndTime)
                .NotEmpty()
                .WithMessage(
                    "End time is required.")
                .GreaterThan(dto =>
                    dto.StartTime)
                .WithMessage(
                    "End time must be after start time.");
        }
    }
}