using FluentValidation;

namespace DriveOS.Modules.CRM.Application.Tasks.CreateTask;

public sealed class CreateCrmTaskCommandValidator : AbstractValidator<CreateCrmTaskCommand>
{
    public CreateCrmTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.DueAtUtc).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
    }
}
