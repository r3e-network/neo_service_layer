using FluentValidation;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.API.Validation
{
    /// <summary>
    /// Validator for function compositions
    /// </summary>
    public class FunctionCompositionValidator : AbstractValidator<FunctionComposition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionCompositionValidator"/> class
        /// </summary>
        public FunctionCompositionValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.ExecutionMode)
                .NotEmpty().WithMessage("Execution mode is required")
                .Must(mode => mode == "sequential" || mode == "parallel")
                .WithMessage("Execution mode must be either 'sequential' or 'parallel'");

            RuleFor(x => x.MaxExecutionTime)
                .GreaterThan(0).WithMessage("Maximum execution time must be greater than 0");

            RuleFor(x => x.ErrorHandlingStrategy)
                .Must(strategy => strategy == null || strategy == "stop" || strategy == "continue" || strategy == "retry")
                .WithMessage("Error handling strategy must be 'stop', 'continue', or 'retry'");

            RuleFor(x => x.Steps)
                .NotEmpty().WithMessage("At least one step is required");

            RuleForEach(x => x.Steps)
                .SetValidator(new FunctionCompositionStepValidator());
        }
    }

    /// <summary>
    /// Validator for function composition steps
    /// </summary>
    public class FunctionCompositionStepValidator : AbstractValidator<FunctionCompositionStep>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionCompositionStepValidator"/> class
        /// </summary>
        public FunctionCompositionStepValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Step name is required")
                .MaximumLength(100).WithMessage("Step name cannot exceed 100 characters");

            RuleFor(x => x.FunctionId)
                .NotEmpty().WithMessage("Function ID is required");

            RuleFor(x => x.TimeoutMs)
                .GreaterThan(0).WithMessage("Timeout must be greater than 0");

            RuleFor(x => x.RetryPolicy)
                .NotNull().WithMessage("Retry policy cannot be null");
        }
    }

    /// <summary>
    /// Validator for function retry policies
    /// </summary>
    public class FunctionRetryPolicyValidator : AbstractValidator<FunctionRetryPolicy>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionRetryPolicyValidator"/> class
        /// </summary>
        public FunctionRetryPolicyValidator()
        {
            RuleFor(x => x.MaxRetries)
                .GreaterThanOrEqualTo(0).WithMessage("Maximum retries must be greater than or equal to 0");

            RuleFor(x => x.InitialDelayMs)
                .GreaterThan(0).WithMessage("Initial delay must be greater than 0");

            RuleFor(x => x.BackoffMultiplier)
                .GreaterThanOrEqualTo(1).WithMessage("Backoff multiplier must be greater than or equal to 1");

            RuleFor(x => x.MaxDelayMs)
                .GreaterThan(0).WithMessage("Maximum delay must be greater than 0")
                .GreaterThanOrEqualTo(x => x.InitialDelayMs).WithMessage("Maximum delay must be greater than or equal to initial delay");
        }
    }
}
