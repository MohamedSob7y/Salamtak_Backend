using FluentValidation;
using Salamtak.Shared.DTOs.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.AI
{
    public class AiSymptomRequestValidator : AbstractValidator<AiSymptomRequestDto>
    {
        public AiSymptomRequestValidator()
        {
            RuleFor(x => x.Symptoms)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(1000);

            RuleFor(x => x.City)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.City));
        }
    }
}
