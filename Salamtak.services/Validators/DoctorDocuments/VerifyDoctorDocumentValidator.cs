using FluentValidation;
using Salamtak.Shared.DTOs.DoctorDocuments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.DoctorDocuments
{
    public class VerifyDoctorDocumentValidator : AbstractValidator<VerifyDoctorDocumentDto>
    {
        public VerifyDoctorDocumentValidator()
        {
            RuleFor(x => x.DocumentId)
                .NotEmpty();

            RuleFor(x => x.AdminId)
                .NotEmpty();
        }
    }
}
