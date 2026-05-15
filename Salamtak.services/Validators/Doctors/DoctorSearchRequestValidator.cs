using FluentValidation;
using Salamtak.Shared.DTOs.Doctors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Doctors
{
    public class DoctorSearchRequestValidator : AbstractValidator<DoctorSearchRequestDto>
    {
        public DoctorSearchRequestValidator()
        {
            RuleFor(x => x.DoctorName)
                .MaximumLength(150)
                .When(x => !string.IsNullOrWhiteSpace(x.DoctorName));

            RuleFor(x => x.City)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.City));

            RuleFor(x => x.MinRating)
                .InclusiveBetween(0, 5)
                .When(x => x.MinRating.HasValue);

            RuleFor(x => x.MinFee)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinFee.HasValue);

            RuleFor(x => x.MaxFee)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MaxFee.HasValue);

            RuleFor(x => x)
                .Must(x => !x.MinFee.HasValue || !x.MaxFee.HasValue || x.MaxFee >= x.MinFee)
                .WithMessage("MaxFee must be greater than or equal to MinFee.");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50);
        }
    }
}
