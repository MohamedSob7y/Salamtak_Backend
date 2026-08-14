using FluentValidation;
using Salamtak.Shared.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Users
{
    public class UpdateUserStatusValidator : AbstractValidator<UpdateUserStatusDto>
    {
        public UpdateUserStatusValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(x => x.Status)
                .NotEmpty()
                .Must(x => x == "Active" || x == "Pending" || x == "Suspended")
                .WithMessage("Invalid user status.");
        }
    }
}
