using FluentValidation;

namespace DriveOS.Modules.Students.Application.Relationships;

public sealed class CreateStudentRelationshipCommandValidator
    : AbstractValidator<CreateStudentRelationshipCommand>
{
    public CreateStudentRelationshipCommandValidator()
    {
        RuleFor(x => x.PartyId).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(254)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(40);
        RuleFor(x => x)
            .Must(x => !x.EffectiveTo.HasValue || x.EffectiveTo >= x.EffectiveFrom)
            .WithErrorCode("Students.Relationship.Period.Invalid");
    }
}

public sealed class UpdateStudentRelationshipCommandValidator
    : AbstractValidator<UpdateStudentRelationshipCommand>
{
    public UpdateStudentRelationshipCommandValidator()
    {
        RuleFor(x => x.RelationshipId).NotEmpty();
        RuleFor(x => x)
            .Must(x => !x.EffectiveTo.HasValue || x.EffectiveTo >= x.EffectiveFrom)
            .WithErrorCode("Students.Relationship.Period.Invalid");
    }
}

public sealed class SuspendStudentRelationshipCommandValidator
    : AbstractValidator<SuspendStudentRelationshipCommand>
{
    public SuspendStudentRelationshipCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class RevokeStudentRelationshipCommandValidator
    : AbstractValidator<RevokeStudentRelationshipCommand>
{
    public RevokeStudentRelationshipCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
