using FluentValidation;
using Salamtak.Shared.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Auth
{

    public class RegisterDoctorRequestValidator : AbstractValidator<RegisterDoctorRequestDto>
    {
        public RegisterDoctorRequestValidator()
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

            RuleFor(x => x.SpecialtyId)
                .NotEmpty();

            RuleFor(x => x.LicenseNumber)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Bio)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.Bio));

            RuleFor(x => x.ConsultationFee)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.ExperienceYears)
                .GreaterThanOrEqualTo(0);
        }
    }
}
