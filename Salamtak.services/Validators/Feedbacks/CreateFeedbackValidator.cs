using FluentValidation;
using Salamtak.Shared.DTOs.Feedbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Feedbacks
{
    public class CreateFeedbackValidator : AbstractValidator<CreateFeedbackDto>
    {
        public CreateFeedbackValidator()
        {
            RuleFor(x => x.AppointmentId)
                .NotEmpty();

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5);

            RuleFor(x => x.Comment)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.Comment));
        }
    }
}
