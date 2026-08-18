using FluentValidation;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Signature.Record;

public sealed class RecordTrainingContractSignatureCommandValidator : AbstractValidator<RecordTrainingContractSignatureCommand>
{
    public RecordTrainingContractSignatureCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.ContractId).Must(x => !x.IsEmpty);
        RuleFor(x => x.SignatureProcessId).Must(x => !x.IsEmpty);
        RuleFor(x => x.SignatoryId).Must(x => !x.IsEmpty);
        RuleFor(x => x.ActorUserId).Must(x => !x.IsEmpty);
        RuleFor(x => x.DocumentSha256).NotEmpty().Length(64);
        RuleFor(x => x.SignatureMethod).NotEmpty().MaximumLength(80);
        RuleFor(x => x.AuthenticationMethod).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Provider).NotEmpty().MaximumLength(120);
        RuleFor(x => x.ProviderSignatureReference).NotEmpty().MaximumLength(250);
        RuleFor(x => x.CertificateReference).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.CertificateReference));
        RuleFor(x => x.IpAddress).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.IpAddress));
        RuleFor(x => x.UserAgent).MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.UserAgent));
        RuleFor(x => x.SignedAtUtc).NotEmpty();
    }
}
