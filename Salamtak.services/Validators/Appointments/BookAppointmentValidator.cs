using FluentValidation;
using Salamtak.Shared.DTOs.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Appointments
{
    public class BookAppointmentValidator : AbstractValidator<BookAppointmentDto>
    {
        public BookAppointmentValidator()
        {
            RuleFor(x => x.DoctorId)
                .NotEmpty();

            RuleFor(x => x.ClinicId)
                .NotEmpty();

            RuleFor(x => x.AvailabilitySlotId)
                .NotEmpty();

            RuleFor(x => x.BookingMethod)
                .NotEmpty()
                .Must(x => x == "Direct" || x == "AI")
                .WithMessage("BookingMethod must be Direct or AI.");

            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Reason));
        }
    }
}
