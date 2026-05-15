using FluentValidation;
using Salamtak.Shared.DTOs.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Appointments
{
    public class CompleteAppointmentValidator : AbstractValidator<CompleteAppointmentDto>
    {
        public CompleteAppointmentValidator()
        {
            RuleFor(x => x.AppointmentId)
                .NotEmpty();

            RuleFor(x => x.Notes)
                .MaximumLength(1500)
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));
        }
    }
}
