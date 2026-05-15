using FluentValidation;
using Salamtak.Shared.DTOs.MedicalReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.MedicalReports
{
    public class UpdateMedicalReportEntryValidator : AbstractValidator<UpdateMedicalReportEntryDto>
    {
        public UpdateMedicalReportEntryValidator()
        {
            RuleFor(x => x.EntryId)
                .NotEmpty()
                .WithMessage("EntryId is required.");

            RuleFor(x => x.Diagnosis)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.Diagnosis));

            RuleFor(x => x.Recommendations)
                .MaximumLength(1500)
                .When(x => !string.IsNullOrWhiteSpace(x.Recommendations));

            RuleFor(x => x.Notes)
                .MaximumLength(1500)
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));

            RuleFor(x => x)
                .Must(x =>
                    !string.IsNullOrWhiteSpace(x.Diagnosis) ||
                    !string.IsNullOrWhiteSpace(x.Recommendations) ||
                    !string.IsNullOrWhiteSpace(x.Notes) ||
                    x.Prescriptions.Any())
                .WithMessage("At least one field must be provided to update the medical report entry.");

            RuleForEach(x => x.Prescriptions)
                .ChildRules(p =>
                {
                    p.RuleFor(x => x.PrescriptionId)
                        .NotEmpty();

                    p.RuleFor(x => x.DrugName)
                        .NotEmpty()
                        .MaximumLength(150);

                    p.RuleFor(x => x.Dose)
                        .MaximumLength(100)
                        .When(x => !string.IsNullOrWhiteSpace(x.Dose));

                    p.RuleFor(x => x.Duration)
                        .MaximumLength(100)
                        .When(x => !string.IsNullOrWhiteSpace(x.Duration));

                    p.RuleFor(x => x.Instructions)
                        .MaximumLength(500)
                        .When(x => !string.IsNullOrWhiteSpace(x.Instructions));
                });
        }
    }
}
