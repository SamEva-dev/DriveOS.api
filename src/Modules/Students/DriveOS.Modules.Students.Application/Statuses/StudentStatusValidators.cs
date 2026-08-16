using FluentValidation;

namespace DriveOS.Modules.Students.Application.Statuses;

public sealed class ApplyStudentBlockCommandValidator : AbstractValidator<ApplyStudentBlockCommand>
{
    public ApplyStudentBlockCommandValidator()
    {
        RuleFor(x => x.BlockType).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.SourceDomain).NotEmpty().MaximumLength(80);
        RuleFor(x => x.BlockingActions).NotEmpty();
        RuleFor(x => x.ExpectedResolution).MaximumLength(500);
    }
}

public sealed class ReleaseStudentBlockCommandValidator
    : AbstractValidator<ReleaseStudentBlockCommand>
{
    public ReleaseStudentBlockCommandValidator()
    {
        RuleFor(x => x.BlockId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class OverrideStudentBlockCommandValidator
    : AbstractValidator<OverrideStudentBlockCommand>
{
    public OverrideStudentBlockCommandValidator()
    {
        RuleFor(x => x.BlockId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.UntilUtc).GreaterThan(DateTimeOffset.UtcNow);
    }
}
