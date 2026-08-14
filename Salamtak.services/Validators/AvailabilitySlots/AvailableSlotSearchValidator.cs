using FluentValidation;
using Salamtak.Shared.DTOs.AvailabilitySlots;

namespace Salamtak.services
    .Validators
    .AvailabilitySlots
{
    public class AvailableSlotSearchValidator
        : AbstractValidator<AvailableSlotSearchDto>
    {
        public AvailableSlotSearchValidator()
        {
            RuleFor(dto => dto.DoctorId)
                .NotEqual(Guid.Empty)
                .When(dto =>
                    dto.DoctorId.HasValue)
                .WithMessage(
                    "Doctor id is invalid.");

            RuleFor(dto => dto.SpecialtyId)
                .NotEqual(Guid.Empty)
                .When(dto =>
                    dto.SpecialtyId.HasValue)
                .WithMessage(
                    "Specialty id is invalid.");

            RuleFor(dto => dto.ClinicId)
                .NotEqual(Guid.Empty)
                .When(dto =>
                    dto.ClinicId.HasValue)
                .WithMessage(
                    "Clinic id is invalid.");

            RuleFor(dto => dto.Date)
                .Must(date =>
                    !date.HasValue ||
                    date.Value.Date >=
                    DateTime.UtcNow.Date)
                .WithMessage(
                    "Date cannot be in the past.");
        }
    }
}