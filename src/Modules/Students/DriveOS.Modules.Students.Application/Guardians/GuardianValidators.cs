using FluentValidation;

namespace DriveOS.Modules.Students.Application.Guardians;

public sealed class CreateGuardianCommandValidator : AbstractValidator<CreateGuardianCommand>
{
    public CreateGuardianCommandValidator()
    {
        RuleFor(x => x.GuardianPersonId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(254)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(40);
        RuleFor(x => x.LegalBasis).NotEmpty().MaximumLength(500);
        RuleFor(x => x.NotificationPreferences).MaximumLength(500);
        RuleFor(x => x)
            .Must(x => !x.EffectiveTo.HasValue || x.EffectiveTo >= x.EffectiveFrom)
            .WithErrorCode("Students.Guardians.InvalidPeriod");
    }
}

public sealed class UpdateGuardianCommandValidator : AbstractValidator<UpdateGuardianCommand>
{
    public UpdateGuardianCommandValidator()
    {
        RuleFor(x => x.RelationshipId).NotEmpty();
        RuleFor(x => x.LegalBasis).NotEmpty().MaximumLength(500);
        RuleFor(x => x.NotificationPreferences).MaximumLength(500);
        RuleFor(x => x)
            .Must(x => !x.EffectiveTo.HasValue || x.EffectiveTo >= x.EffectiveFrom)
            .WithErrorCode("Students.Guardians.InvalidPeriod");
    }
}

public sealed class RevokeGuardianCommandValidator : AbstractValidator<RevokeGuardianCommand>
{
    public RevokeGuardianCommandValidator()
    {
        RuleFor(x => x.RelationshipId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
