using FluentValidation;
using Salamtak.Shared.DTOs.Appointments;

namespace Salamtak.services
    .Validators
    .Appointments
{
    public class BookAppointmentValidator
        : AbstractValidator<BookAppointmentDto>
    {
        public BookAppointmentValidator()
        {
            RuleFor(dto => dto.DoctorId)
                .NotEmpty()
                .WithMessage(
                    "Doctor id is required.");

            RuleFor(dto => dto.ClinicId)
                .NotEmpty()
                .WithMessage(
                    "Clinic id is required.");

            RuleFor(dto =>
                    dto.AvailabilitySlotId)
                .NotEmpty()
                .WithMessage(
                    "Availability slot id is required.");

            RuleFor(dto => dto.BookingMethod)
                .NotEmpty()
                .WithMessage(
                    "Booking method is required.")
                .Must(method =>
                    string.Equals(
                        method,
                        "Direct",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                        method,
                        "AI",
                        StringComparison.OrdinalIgnoreCase))
                .WithMessage(
                    "BookingMethod must be Direct or AI.");

            RuleFor(dto => dto.Reason)
                .MaximumLength(500)
                .When(dto =>
                    !string.IsNullOrWhiteSpace(
                        dto.Reason));
        }
    }
}