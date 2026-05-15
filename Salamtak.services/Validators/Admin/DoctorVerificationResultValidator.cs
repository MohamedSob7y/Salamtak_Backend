using FluentValidation;
using Salamtak.Shared.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Admin
{
    public class DoctorVerificationResultValidator : AbstractValidator<DoctorVerificationResultDto>
    {
        public DoctorVerificationResultValidator()
        {
            RuleFor(x => x.DoctorId)
                .NotEmpty()
                .WithMessage("DoctorId is required.");

            RuleFor(x => x.RejectionReason)
                .NotEmpty()
                .MaximumLength(500)
                .When(x => !x.IsApproved)
                .WithMessage("Rejection reason is required when doctor is rejected.");
        }
    }
}
