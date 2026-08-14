using FluentValidation;
using Salamtak.Shared.DTOs.DoctorDocuments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.DoctorDocuments
{
    public class RejectDoctorDocumentValidator
          : AbstractValidator<RejectDoctorDocumentDto>
    {
        public RejectDoctorDocumentValidator()
        {
            RuleFor(x => x.RejectionReason)
                .NotEmpty()
                .WithMessage(
                    "Rejection reason is required.")
                .MaximumLength(500)
                .WithMessage(
                    "Rejection reason must not exceed 500 characters.");
        }
    }
}
