using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Audit;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.Organizations.Lifecycle;

public sealed class ChangeOrganizationStatusCommandHandler(
    IOrganizationRepository organizationRepository,
    IOrganizationActivationReadinessService activationReadinessService,
    IOrganizationActivationReadinessReportCache readinessCache,
    IOrganizationActivationReadinessAuditSink readinessAuditSink,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IClock clock
) : ICommandHandler<ChangeOrganizationStatusCommand>
{
    public async Task<Result> Handle(
        ChangeOrganizationStatusCommand command,
        CancellationToken cancellationToken
    )
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Result.Failure(OrganizationErrors.CurrentUserRequired);
        }

        OrganizationId organizationId = new(command.OrganizationId);

        Organization? organization = await organizationRepository.GetByIdAsync(
            organizationId,
            asNoTracking: false,
            cancellationToken
        );

        if (organization is null)
        {
            return Result.Failure(OrganizationErrors.NotFoundById(organizationId));
        }

        if (command.TargetStatus == OrganizationStatus.Active)
        {
            // Never use the display cache for an authorization/business decision.
            OrganizationActivationReadinessReport report =
                await activationReadinessService.EvaluateAsync(organizationId, cancellationToken);

            await readinessAuditSink.WriteAsync(
                new OrganizationActivationReadinessAuditEntry(
                    organizationId,
                    currentUser.UserId.Value.Value,
                    report.IsReady
                        ? "OrganizationActivationReadinessPassed"
                        : "OrganizationActivationReadinessBlocked",
                    report.IsReady,
                    report.BlockingRequirements.Select(x => x.Code).ToArray(),
                    clock.UtcNow
                ),
                cancellationToken
            );

            if (!report.IsReady)
            {
                return Result.Failure(
                    OrganizationActivationReadinessErrors.RequirementsNotMet(
                        report.BlockingRequirements
                    )
                );
            }
        }

        OrganizationStatus currentStatus = organization.Status;
        OrganizationStatusChangeReason reason = OrganizationStatusChangeReason.Create(
            command.Reason
        );

        Result transitionResult = ApplyTransition(
            organization,
            command.TargetStatus,
            reason,
            currentUser.UserId.Value.Value,
            clock.UtcNow,
            currentStatus
        );

        if (transitionResult.IsFailure)
        {
            return transitionResult;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        readinessCache.Invalidate(organizationId);

        return Result.Success();
    }

    private static Result ApplyTransition(
        Organization organization,
        OrganizationStatus targetStatus,
        OrganizationStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc,
        OrganizationStatus currentStatus
    )
    {
        try
        {
            switch (targetStatus)
            {
                case OrganizationStatus.PendingActivation:
                    organization.SubmitForActivation(reason, changedByUserId, changedAtUtc);
                    break;
                case OrganizationStatus.Active:
                    organization.Activate(reason, changedByUserId, changedAtUtc);
                    break;
                case OrganizationStatus.Restricted:
                    organization.Restrict(reason, changedByUserId, changedAtUtc);
                    break;
                case OrganizationStatus.Suspended:
                    organization.Suspend(reason, changedByUserId, changedAtUtc);
                    break;
                case OrganizationStatus.Closed:
                    organization.Close(reason, changedByUserId, changedAtUtc);
                    break;
                default:
                    return Result.Failure(
                        OrganizationErrors.InvalidStatusTransition(currentStatus, targetStatus)
                    );
            }

            return Result.Success();
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(
                OrganizationErrors.InvalidStatusTransition(currentStatus, targetStatus)
            );
        }
    }
}
