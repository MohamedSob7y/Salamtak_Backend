using FluentValidation;
using Salamtak.Shared.DTOs.Appointments;

namespace Salamtak.services
    .Validators
    .Appointments
{
    public class CancelAppointmentValidator
        : AbstractValidator<CancelAppointmentDto>
    {
        public CancelAppointmentValidator()
        {
            RuleFor(dto =>
                    dto.AppointmentId)
                .NotEmpty()
                .WithMessage(
                    "Appointment id is required.");

            RuleFor(dto =>
                    dto.CancelReason)
                .MaximumLength(500)
                .When(dto =>
                    !string.IsNullOrWhiteSpace(
                        dto.CancelReason));
        }
    }
}