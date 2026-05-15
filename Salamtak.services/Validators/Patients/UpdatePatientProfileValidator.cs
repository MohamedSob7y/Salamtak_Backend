using FluentValidation;
using Salamtak.Shared.DTOs.Patients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Patients
{
    public class UpdatePatientProfileValidator : AbstractValidator<UpdatePatientProfileDto>
    {
        public UpdatePatientProfileValidator()
        {
            RuleFor(x => x.DateOfBirth)
                .NotEmpty()
                .LessThan(DateTime.UtcNow)
                .WithMessage("Date of birth must be in the past.");

            RuleFor(x => x.Gender)
                .NotEmpty()
                .Must(g => g == "Male" || g == "Female")
                .WithMessage("Gender must be Male or Female.");

            RuleFor(x => x.Address)
                .MaximumLength(250)
                .When(x => !string.IsNullOrWhiteSpace(x.Address));

            RuleFor(x => x.BloodType)
                .MaximumLength(5)
                .When(x => !string.IsNullOrWhiteSpace(x.BloodType));

            RuleFor(x => x.Height)
                .GreaterThan(0)
                .When(x => x.Height.HasValue);

            RuleFor(x => x.Weight)
                .GreaterThan(0)
                .When(x => x.Weight.HasValue);
        }
    }
}
