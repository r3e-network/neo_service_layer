using FluentValidation;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.API.Validation
{
    /// <summary>
    /// Validator for function access policies
    /// </summary>
    public class FunctionAccessPolicyValidator : AbstractValidator<FunctionAccessPolicy>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionAccessPolicyValidator"/> class
        /// </summary>
        public FunctionAccessPolicyValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.FunctionId)
                .NotEmpty().WithMessage("Function ID is required");

            RuleFor(x => x.Rules)
                .NotEmpty().WithMessage("At least one rule is required");
        }
    }
}
