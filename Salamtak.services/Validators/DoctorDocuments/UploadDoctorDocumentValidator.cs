using FluentValidation;
using Salamtak.Domain.Models.Enums;
using Salamtak.Shared.DTOs.DoctorDocuments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.DoctorDocuments
{
    public class UploadDoctorDocumentValidator
       : AbstractValidator<UploadDoctorDocumentDto>
    {
        public UploadDoctorDocumentValidator()
        {
            RuleFor(x => x.DocumentType)
                .NotEmpty()
                .Must(type =>
                    Enum.TryParse<DoctorDocumentType>(
                        type,
                        true,
                        out _))
                .WithMessage(
                    "Invalid document type. Allowed values: License, Certificate, CV, SyndicateCard, NationalId, Other.");

            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("Document file is required.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x =>
                    !string.IsNullOrWhiteSpace(
                        x.Description));
        }
    }
}
