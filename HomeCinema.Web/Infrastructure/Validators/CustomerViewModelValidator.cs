using System;
using FluentValidation;
using HomeCinema.Web.Models;

namespace HomeCinema.Web.Infrastructure.Validators
{
    public class CustomerViewModelValidator : AbstractValidator<CustomerViewModel>
    {
        public CustomerViewModelValidator()
        {
            RuleFor(c => c.FirstName).NotEmpty().Length(1, 100).WithMessage("First Name must be 1 - 100 characters");
            RuleFor(c => c.LastName).NotEmpty().Length(1, 100).WithMessage("Last Name must be between 1 - 100 characters");
            RuleFor(c => c.IdentityCard).NotEmpty().Length(1, 100).WithMessage("Identity Card must be between 1 - 50 characters");
            RuleFor(c => c.DateOfBirth).NotNull().LessThan(DateTime.Now.AddYears(-16)).WithMessage("Customer must be at least 16 years old");
            RuleFor(c => c.Mobile).NotEmpty().Matches(@"^\d{10}$").Length(10).WithMessage("Mobile phone must have 10 digits");
            RuleFor(c => c.Email).NotEmpty().EmailAddress().WithMessage("Enter a valid Email address");
        }
    }
}
