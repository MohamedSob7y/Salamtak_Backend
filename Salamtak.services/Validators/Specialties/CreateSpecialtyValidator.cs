using FluentValidation;
using Salamtak.Shared.DTOs.Specialties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Specialties
{
    public class CreateSpecialtyValidator : AbstractValidator<CreateSpecialtyDto>
    {
        public CreateSpecialtyValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
