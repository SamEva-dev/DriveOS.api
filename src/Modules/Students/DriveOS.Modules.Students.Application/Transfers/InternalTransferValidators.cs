using DriveOS.Modules.Students.Domain.Transfers;
using FluentValidation;

namespace DriveOS.Modules.Students.Application.Transfers;

public sealed class AnalyzeInternalTransferCommandValidator
    : AbstractValidator<AnalyzeInternalTransferCommand>
{
    public AnalyzeInternalTransferCommandValidator()
    {
        RuleFor(x => x.TargetBranchId).NotEmpty();
        RuleFor(x => x.Elements).NotEqual(InternalTransferElement.None);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.EffectiveOn).NotNull().When(x => x.Mode != InternalTransferMode.Immediate);
        RuleFor(x => x.TemporaryUntil)
            .NotNull()
            .When(x => x.Mode == InternalTransferMode.Temporary);
    }
}

public sealed class ValidateInternalTransferCommandValidator
    : AbstractValidator<ValidateInternalTransferCommand>
{
    public ValidateInternalTransferCommandValidator() => RuleFor(x => x.TransferId).NotEmpty();
}
