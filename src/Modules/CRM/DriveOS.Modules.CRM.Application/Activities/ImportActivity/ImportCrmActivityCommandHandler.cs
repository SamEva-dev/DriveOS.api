using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Activities.ImportActivity;

public sealed class ImportCrmActivityCommandHandler(
    ILeadRepository leads,
    ICrmActivityRepository activities,
    ICrmActivityImportLock importLock,
    ICrmUnitOfWork unitOfWork,
    IClock clock
) : ICommandHandler<ImportCrmActivityCommand, ImportCrmActivityResult>
{
    public async Task<Result<ImportCrmActivityResult>> Handle(
        ImportCrmActivityCommand command,
        CancellationToken ct
    )
    {
        if (command.OccurredAtUtc > clock.UtcNow.AddMinutes(1))
            return Result.Failure<ImportCrmActivityResult>(CrmActivityErrors.OccurredAtInFuture);

        string key = (command.IdempotencyKey ?? string.Empty).Trim();
        if (key.Length == 0)
            return Result.Failure<ImportCrmActivityResult>(CrmActivityErrors.MetadataInvalid);
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await importLock.AcquireAsync(command.OrganizationId, key, ct);
            CrmActivity? existing = await activities.GetByIdempotencyKeyAsync(
                command.OrganizationId,
                key,
                ct
            );
            if (existing is not null)
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                return Result.Success(new ImportCrmActivityResult(existing.Id.Value, true));
            }

            if (
                command.LeadId.HasValue
                && await leads.GetByIdAsync(command.OrganizationId, command.LeadId.Value, ct)
                    is null
            )
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                return Result.Failure<ImportCrmActivityResult>(LeadErrors.NotFound);
            }

            CrmActivityMetadata metadata = CrmActivityMetadata.Imported(
                command.ExternalId,
                key,
                command.SyncStatus,
                clock.UtcNow,
                command.SyncErrorKey,
                command.Result,
                command.DurationMinutes,
                command.RequiresRegularization,
                command.AttachmentName,
                command.AttachmentReference
            );
            Result<CrmActivity> activity = CrmActivity.Create(
                CrmActivityId.New(),
                command.OrganizationId,
                command.LeadId,
                command.Type,
                command.Direction,
                command.Subject,
                command.Details,
                command.OccurredAtUtc,
                command.AdvisorUserId,
                metadata
            );
            if (activity.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                return Result.Failure<ImportCrmActivityResult>(activity.Error);
            }

            activities.Add(activity.Value);
            await unitOfWork.CommitTransactionAsync(ct);
            return Result.Success(new ImportCrmActivityResult(activity.Value.Id.Value, false));
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
