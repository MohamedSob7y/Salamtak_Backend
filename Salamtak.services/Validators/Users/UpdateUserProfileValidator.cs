using FluentValidation;
using Salamtak.Shared.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Validators.Users
{
    public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileDto>
    {
        public UpdateUserProfileValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .MaximumLength(150)
                .WithMessage("Full name must not exceed 150 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(20)
                .Matches(@"^\+?[0-9]{10,15}$")
                .WithMessage("Invalid phone number format.");
        }
    }
}
