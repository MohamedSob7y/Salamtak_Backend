using FluentValidation;
using Salamtak.Shared.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Admin
{
    public class DoctorVerificationResultValidator
         : AbstractValidator<DoctorVerificationResultDto>
    {
        public DoctorVerificationResultValidator()
        {
            RuleFor(dto => dto.DoctorId)
                .NotEmpty()
                .WithMessage(
                    "DoctorId is required.");

            RuleFor(dto => dto.RejectionReason)
                .NotEmpty()
                .WithMessage(
                    "Rejection reason is required when doctor is rejected.")
                .MaximumLength(500)
                .WithMessage(
                    "Rejection reason must not exceed 500 characters.")
                .When(dto => !dto.IsApproved);
        }
    }
}
