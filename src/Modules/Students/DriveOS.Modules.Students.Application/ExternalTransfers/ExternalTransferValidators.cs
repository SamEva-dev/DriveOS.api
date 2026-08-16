using DriveOS.Modules.Students.Domain.ExternalTransfers;
using FluentValidation;

namespace DriveOS.Modules.Students.Application.ExternalTransfers;

public sealed class CreateExternalTransferCommandValidator
    : AbstractValidator<CreateExternalTransferCommand>
{
    public CreateExternalTransferCommandValidator()
    {
        RuleFor(x => x.TargetOrganizationId).NotEmpty();
        RuleFor(x => x.DataScope).NotEqual(ExternalTransferDataScope.None);
        RuleFor(x => x.CountryCode).NotEmpty().Length(2);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Responsibilities).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.TemporaryUntil)
            .NotNull()
            .When(x => x.Type == ExternalTransferType.TemporaryTransfer);
    }
}

public sealed class VerifyExternalTransferConsentCommandValidator
    : AbstractValidator<VerifyExternalTransferConsentCommand>
{
    public VerifyExternalTransferConsentCommandValidator()
    {
        RuleFor(x => x.TransferId).NotEmpty();
        RuleFor(x => x.EvidenceReference).NotEmpty().MaximumLength(500);
    }
}

public sealed class ReviewExternalTransferFinanceCommandValidator
    : AbstractValidator<ReviewExternalTransferFinanceCommand>
{
    public ReviewExternalTransferFinanceCommandValidator() => RuleFor(x => x.TransferId).NotEmpty();
}

public sealed class SubmitExternalTransferCommandValidator
    : AbstractValidator<SubmitExternalTransferCommand>
{
    public SubmitExternalTransferCommandValidator() => RuleFor(x => x.TransferId).NotEmpty();
}

public sealed class DecideExternalTransferCommandValidator
    : AbstractValidator<DecideExternalTransferCommand>
{
    public DecideExternalTransferCommandValidator()
    {
        RuleFor(x => x.TransferId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
