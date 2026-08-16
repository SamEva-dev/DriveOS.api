using FluentValidation;

namespace DriveOS.Modules.Students.Application.Branches;

public sealed class AssignStudentBranchCommandValidator
    : AbstractValidator<AssignStudentBranchCommand>
{
    public AssignStudentBranchCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue);
    }
}

public sealed class AnalyzePrimaryBranchChangeCommandValidator
    : AbstractValidator<AnalyzePrimaryBranchChangeCommand>
{
    public AnalyzePrimaryBranchChangeCommandValidator() =>
        RuleFor(x => x.TargetBranchId).NotEmpty();
}

public sealed class ChangePrimaryStudentBranchCommandValidator
    : AbstractValidator<ChangePrimaryStudentBranchCommand>
{
    public ChangePrimaryStudentBranchCommandValidator()
    {
        RuleFor(x => x.AnalysisId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
