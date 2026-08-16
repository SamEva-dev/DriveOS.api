using DriveOS.Modules.Students.Domain.Students;
using FluentValidation;

namespace DriveOS.Modules.Students.Application.Students.Identity;

public sealed class UpdateStudentIdentityCommandValidator
    : AbstractValidator<UpdateStudentIdentityCommand>
{
    public UpdateStudentIdentityCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.StudentId).Must(x => !x.IsEmpty);
        RuleFor(x => x.ActorUserId).Must(x => !x.IsEmpty);
        RuleFor(x => x.Identity.LegalFirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Identity.LegalLastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Identity.PreferredName).MaximumLength(100);
        RuleFor(x => x.Identity.Email)
            .EmailAddress()
            .MaximumLength(254)
            .When(x => !string.IsNullOrWhiteSpace(x.Identity.Email));
        RuleFor(x => x.Identity.Phone).MaximumLength(40);
        RuleFor(x => x.Identity.CountryCode).MaximumLength(3);
        RuleFor(x => x.Identity.PreferredLanguage).MaximumLength(10);
        RuleFor(x => x.Justification).MaximumLength(500);
    }
}

public sealed class VerifyStudentIdentityCommandValidator
    : AbstractValidator<VerifyStudentIdentityCommand>
{
    public VerifyStudentIdentityCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.StudentId).Must(x => !x.IsEmpty);
        RuleFor(x => x.ActorUserId).Must(x => !x.IsEmpty);
        RuleFor(x => x.Status)
            .Must(x =>
                x
                    is IdentityVerificationStatus.DocumentVerified
                        or IdentityVerificationStatus.ExternallyVerified
            );
        RuleFor(x => x.Justification).NotEmpty().MinimumLength(10).MaximumLength(500);
    }
}

public sealed class UpdateOwnStudentContactCommandValidator
    : AbstractValidator<UpdateOwnStudentContactCommand>
{
    public UpdateOwnStudentContactCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.StudentId).Must(x => !x.IsEmpty);
        RuleFor(x => x.ActorUserId).Must(x => !x.IsEmpty);
        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(254)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(40);
        RuleFor(x => x.AddressLine1).MaximumLength(200);
        RuleFor(x => x.AddressLine2).MaximumLength(200);
        RuleFor(x => x.PostalCode).MaximumLength(20);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.CountryCode).MaximumLength(3);
        RuleFor(x => x.PreferredLanguage).MaximumLength(10);
        RuleFor(x => x.TimeZone).MaximumLength(100);
    }
}
