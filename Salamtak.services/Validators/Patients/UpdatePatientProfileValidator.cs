using FluentValidation;
using Salamtak.Domain.Models.Enums;
using Salamtak.Shared.DTOs.Patients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Patients
{
    public class UpdatePatientProfileValidator
        : AbstractValidator<UpdatePatientProfileDto>
    {
        private static readonly HashSet<string>
            AllowedBloodTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "A+",
                "A-",
                "B+",
                "B-",
                "AB+",
                "AB-",
                "O+",
                "O-"
            };

        public UpdatePatientProfileValidator()
        {
            RuleFor(x => x.DateOfBirth)
                .NotEmpty()
                .WithMessage(
                    "Date of birth is required.")
                .LessThan(DateTime.UtcNow.Date)
                .WithMessage(
                    "Date of birth must be in the past.");

            RuleFor(x => x.Gender)
                .NotEmpty()
                .WithMessage(
                    "Gender is required.")
                .Must(BeValidGender)
                .WithMessage(
                    "Gender must be Male or Female.");

            RuleFor(x => x.Address)
                .MaximumLength(250)
                .WithMessage(
                    "Address cannot exceed 250 characters.")
                .When(x =>
                    !string.IsNullOrWhiteSpace(
                        x.Address));

            RuleFor(x => x.BloodType)
                .MaximumLength(5)
                .WithMessage(
                    "Blood type cannot exceed 5 characters.")
                .Must(BeValidBloodType)
                .WithMessage(
                    "Blood type must be one of: A+, A-, B+, B-, AB+, AB-, O+, O-.")
                .When(x =>
                    !string.IsNullOrWhiteSpace(
                        x.BloodType));

            RuleFor(x => x.Height)
                .Must(BeValidPositiveNumber)
                .WithMessage(
                    "Height must be a valid number greater than zero.")
                .When(x =>
                    x.Height.HasValue);

            RuleFor(x => x.Weight)
                .Must(BeValidPositiveNumber)
                .WithMessage(
                    "Weight must be a valid number greater than zero.")
                .When(x =>
                    x.Weight.HasValue);
        }

        private static bool BeValidGender(
            string gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
            {
                return false;
            }

            return Enum.TryParse<Gender>(
                       gender.Trim(),
                       true,
                       out var parsedGender)
                   &&
                   Enum.IsDefined(
                       typeof(Gender),
                       parsedGender);
        }

        private static bool BeValidBloodType(
            string? bloodType)
        {
            if (string.IsNullOrWhiteSpace(
                    bloodType))
            {
                return true;
            }

            return AllowedBloodTypes.Contains(
                bloodType.Trim());
        }

        private static bool BeValidPositiveNumber(
            double? value)
        {
            if (!value.HasValue)
            {
                return true;
            }

            return value.Value > 0
                   && !double.IsNaN(value.Value)
                   && !double.IsInfinity(value.Value);
        }
    }
}
