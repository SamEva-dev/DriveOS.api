using DriveOS.Modules.CRM.Domain.Activities;
using FluentValidation;

namespace DriveOS.Modules.CRM.Application.Activities.ImportActivity;

public sealed class ImportCrmActivityCommandValidator : AbstractValidator<ImportCrmActivityCommand>
{
    public ImportCrmActivityCommandValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Details).MaximumLength(4000);
        RuleFor(x => x.ExternalId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(0, 1440)
            .When(x => x.DurationMinutes.HasValue);
        RuleFor(x => x.SyncErrorKey)
            .NotEmpty()
            .When(x => x.SyncStatus == CrmActivitySyncStatus.Failed);
        RuleFor(x => x.SyncStatus)
            .Must(x =>
                x
                    is CrmActivitySyncStatus.Pending
                        or CrmActivitySyncStatus.Synchronized
                        or CrmActivitySyncStatus.Failed
            );
    }
}
