using FluentValidation;
using Salamtak.Shared.DTOs.Doctors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Doctors
{
    public class DoctorSearchRequestValidator
         : AbstractValidator<DoctorSearchRequestDto>
    {
        public DoctorSearchRequestValidator()
        {
            RuleFor(x => x.SpecialtyId)
                .NotNull()
                .WithMessage("Specialty is required.")
                .Must(specialtyId =>
                    specialtyId.HasValue &&
                    specialtyId.Value != Guid.Empty)
                .WithMessage("Specialty is invalid.");

            RuleFor(x => x.Latitude)
                .NotNull()
                .WithMessage("Latitude is required.")
                .Must(latitude =>
                    latitude.HasValue &&
                    latitude.Value >= -90 &&
                    latitude.Value <= 90)
                .WithMessage("Latitude must be between -90 and 90.");

            RuleFor(x => x.Longitude)
                .NotNull()
                .WithMessage("Longitude is required.")
                .Must(longitude =>
                    longitude.HasValue &&
                    longitude.Value >= -180 &&
                    longitude.Value <= 180)
                .WithMessage("Longitude must be between -180 and 180.");

            RuleFor(x => x.MaxDistanceKm)
                .GreaterThan(0)
                .WithMessage("Maximum distance must be greater than zero.")
                .LessThanOrEqualTo(200)
                .WithMessage(
                    "Maximum distance cannot exceed 200 kilometers.")
                .When(x => x.MaxDistanceKm.HasValue);

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than zero.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50)
                .WithMessage("Page size must be between 1 and 50.");
        }
    }
}
