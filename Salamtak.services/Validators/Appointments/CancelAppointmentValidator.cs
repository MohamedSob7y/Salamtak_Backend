using FluentValidation;
using Salamtak.Shared.DTOs.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Appointments
{
    public class CancelAppointmentValidator : AbstractValidator<CancelAppointmentDto>
    {
        public CancelAppointmentValidator()
        {
            RuleFor(x => x.AppointmentId)
                .NotEmpty();

            RuleFor(x => x.CancelReason)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.CancelReason));
        }
    }
}
