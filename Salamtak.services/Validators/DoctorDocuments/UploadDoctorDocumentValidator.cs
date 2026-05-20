using FluentValidation;
using Salamtak.Shared.DTOs.DoctorDocuments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.DoctorDocuments
{
    public class UploadDoctorDocumentValidator : AbstractValidator<UploadDoctorDocumentDto>
    {
        public UploadDoctorDocumentValidator()
        {
            RuleFor(x => x.DocumentType)
                .NotEmpty()
                .Must(x =>
                    x == "License" ||
                    x == "Certificate" ||
                    x == "CV" ||
                    x == "SyndicateCard" ||
                    x == "NationalId" ||
                    x == "Other")
                .WithMessage("Invalid document type.");

            RuleFor(x => x.FileUrl)
                .NotEmpty()
                .MaximumLength(500);
        }
    }
}
