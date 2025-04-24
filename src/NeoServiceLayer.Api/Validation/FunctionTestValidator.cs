using FluentValidation;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.API.Validation
{
    /// <summary>
    /// Validator for function tests
    /// </summary>
    public class FunctionTestValidator : AbstractValidator<FunctionTest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionTestValidator"/> class
        /// </summary>
        public FunctionTestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.FunctionId)
                .NotEmpty().WithMessage("Function ID is required");

            RuleFor(x => x.InputParameters)
                .NotNull().WithMessage("Input parameters are required");

            RuleFor(x => x.ExpectedOutput)
                .NotNull().WithMessage("Expected output is required");
        }
    }
}
