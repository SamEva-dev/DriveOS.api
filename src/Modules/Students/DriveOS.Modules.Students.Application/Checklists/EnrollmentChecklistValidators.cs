using FluentValidation;

namespace DriveOS.Modules.Students.Application.Checklists;

public sealed class ChangeChecklistItemStatusCommandValidator
    : AbstractValidator<ChangeChecklistItemStatusCommand>
{
    public ChangeChecklistItemStatusCommandValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public sealed class ConfigureChecklistRuleCommandValidator
    : AbstractValidator<ConfigureChecklistRuleCommand>
{
    public ConfigureChecklistRuleCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(80);
        RuleFor(x => x.LabelKey).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TrainingCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TargetRoute).NotEmpty().MaximumLength(300);
        RuleFor(x => x.DueInDays).InclusiveBetween(0, 3650);
    }
}
