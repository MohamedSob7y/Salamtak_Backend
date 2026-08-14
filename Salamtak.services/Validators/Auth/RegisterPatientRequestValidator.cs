using FluentValidation;
using Salamtak.Shared.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Auth
{
    public class RegisterPatientRequestValidator : AbstractValidator<RegisterPatientRequestDto>
    {
        public RegisterPatientRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(150);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(20)
                .Matches(@"^\+?[0-9]{10,15}$")
                .WithMessage("Invalid phone number format.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6);

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
