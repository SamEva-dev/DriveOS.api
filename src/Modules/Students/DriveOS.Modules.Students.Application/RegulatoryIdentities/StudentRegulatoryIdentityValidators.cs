using FluentValidation;

namespace DriveOS.Modules.Students.Application.RegulatoryIdentities;

public sealed class DeclareStudentRegulatoryIdentityCommandValidator
    : AbstractValidator<DeclareStudentRegulatoryIdentityCommand>
{
    public DeclareStudentRegulatoryIdentityCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.StudentId).Must(x => !x.IsEmpty);
        RuleFor(x => x.ActorUserId).Must(x => !x.IsEmpty);
        RuleFor(x => x.CountryCode).NotEmpty().Length(2);
        RuleFor(x => x.IdentifierType).NotEmpty().MinimumLength(2).MaximumLength(40);
        RuleFor(x => x.Value).NotEmpty().MaximumLength(100);
    }
}

public sealed class VerifyStudentRegulatoryIdentityCommandValidator
    : AbstractValidator<VerifyStudentRegulatoryIdentityCommand>
{
    public VerifyStudentRegulatoryIdentityCommandValidator()
    {
        RuleFor(x => x.IdentityId).Must(x => !x.IsEmpty);
        RuleFor(x => x.VerificationMethod).NotEmpty().MinimumLength(2).MaximumLength(80);
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public sealed class RejectStudentRegulatoryIdentityCommandValidator
    : AbstractValidator<RejectStudentRegulatoryIdentityCommand>
{
    public RejectStudentRegulatoryIdentityCommandValidator()
    {
        RuleFor(x => x.IdentityId).Must(x => !x.IsEmpty);
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(3).MaximumLength(500);
    }
}
