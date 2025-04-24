using FluentValidation;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.API.Validation
{
    /// <summary>
    /// Validator for function marketplace items
    /// </summary>
    public class FunctionMarketplaceItemValidator : AbstractValidator<FunctionMarketplaceItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionMarketplaceItemValidator"/> class
        /// </summary>
        public FunctionMarketplaceItemValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.FunctionId)
                .NotEmpty().WithMessage("Function ID is required");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .When(x => !x.IsFree && x.Price > 0);

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Category is required")
                .MaximumLength(50).WithMessage("Category cannot exceed 50 characters");

            RuleForEach(x => x.Tags)
                .MaximumLength(50).WithMessage("Tag cannot exceed 50 characters")
                .When(x => x.Tags != null);
        }
    }
}
