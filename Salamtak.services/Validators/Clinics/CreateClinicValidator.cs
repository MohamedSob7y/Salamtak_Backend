using FluentValidation;
using Salamtak.Shared.DTOs.Clinics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Clinics
{
    public class CreateClinicValidator : AbstractValidator<CreateClinicDto>
    {
        public CreateClinicValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Clinic name is required.")
                .MaximumLength(150)
                .WithMessage(
                    "Clinic name cannot exceed 150 characters.");

            RuleFor(x => x.Address)
                .NotEmpty()
                .WithMessage("Clinic address is required.")
                .MaximumLength(250)
                .WithMessage(
                    "Clinic address cannot exceed 250 characters.");

            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage("Clinic city is required.")
                .MaximumLength(100)
                .WithMessage(
                    "Clinic city cannot exceed 100 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Clinic phone number is required.")
                .MaximumLength(20)
                .WithMessage(
                    "Clinic phone number cannot exceed 20 characters.");
        }
    }
}
