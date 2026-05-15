using FluentValidation;
using Salamtak.Shared.DTOs.DoctorDocuments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.DoctorDocuments
{
    public class RejectDoctorDocumentValidator : AbstractValidator<RejectDoctorDocumentDto>
    {
        public RejectDoctorDocumentValidator()
        {
            RuleFor(x => x.DocumentId)
                .NotEmpty();

            RuleFor(x => x.AdminId)
                .NotEmpty();

            RuleFor(x => x.RejectionReason)
                .NotEmpty()
                .MaximumLength(500);
        }
    }
}
