using FluentValidation;
using Salamtak.Shared.DTOs.Appointments;

namespace Salamtak.services
    .Validators
    .Appointments
{
    public class CompleteAppointmentValidator
        : AbstractValidator<CompleteAppointmentDto>
    {
        public CompleteAppointmentValidator()
        {
            RuleFor(dto =>
                    dto.AppointmentId)
                .NotEmpty()
                .WithMessage(
                    "Appointment id is required.");

            RuleFor(dto => dto.Notes)
                .MaximumLength(1500)
                .When(dto =>
                    !string.IsNullOrWhiteSpace(
                        dto.Notes));
        }
    }
}