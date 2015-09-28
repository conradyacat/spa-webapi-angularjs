using System;
using FluentValidation;
using HomeCinema.Web.Models;

namespace HomeCinema.Web.Infrastructure.Validators
{
    public class StockViewModelValidator : AbstractValidator<StockViewModel>
    {
        public StockViewModelValidator()
        {
            RuleFor(s => s.ID).GreaterThan(0).WithMessage("Invalid stock item");
            RuleFor(s => s.UniqueKey).NotEqual(Guid.Empty).WithMessage("Invalid stock item");
        }
    }
}
