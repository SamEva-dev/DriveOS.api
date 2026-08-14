using FluentValidation;

namespace DriveOS.Modules.CRM.Application.Activities.CreateActivity;

public sealed class CreateCrmActivityCommandValidator : AbstractValidator<CreateCrmActivityCommand>
{
    public CreateCrmActivityCommandValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Details).MaximumLength(4000);
        RuleFor(x => x.OccurredAtUtc).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Direction).IsInEnum();
        RuleFor(x => x.NextActionTitle).MaximumLength(200);
        RuleFor(x => x.NextActionType).IsInEnum();
        RuleFor(x => x.NextActionDueAtUtc).NotNull()
            .When(x => !string.IsNullOrWhiteSpace(x.NextActionTitle));
        RuleFor(x => x.NextActionTitle).NotEmpty()
            .When(x => x.NextActionDueAtUtc.HasValue);
        RuleFor(x => x.LeadId).NotNull()
            .When(x => !string.IsNullOrWhiteSpace(x.NextActionTitle) || x.NextActionDueAtUtc.HasValue);
    }
}
