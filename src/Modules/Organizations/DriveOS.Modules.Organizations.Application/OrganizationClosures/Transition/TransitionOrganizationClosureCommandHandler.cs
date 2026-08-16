using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Commands;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Readiness;
using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.Transition;

internal sealed class TransitionOrganizationClosureCommandHandler(
    IOrganizationClosureRepository repository,
    IOrganizationClosureReadinessService readinessService,
    IOrganizationClosureOrchestrator orchestrator,
    IOrganizationClosureAuditSink auditSink,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IClock clock
) : ICommandHandler<TransitionOrganizationClosureCommand>
{
    public async Task<Result> Handle(
        TransitionOrganizationClosureCommand command,
        CancellationToken cancellationToken
    )
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure(OrganizationClosureErrors.CurrentUserRequired);

        OrganizationClosure? closure = await repository.GetForUpdateAsync(
            command.ClosureId,
            cancellationToken
        );
        if (closure is null)
            return Result.Failure(OrganizationClosureErrors.NotFound);

        var actor = currentUser.UserId.Value;
        Result transition;

        switch (command.Action)
        {
            case OrganizationClosureAction.Submit:
                transition = closure.SubmitForReview(actor);
                break;
            case OrganizationClosureAction.Approve:
                transition = closure.Approve(actor, command.Comment, clock.UtcNow);
                break;
            case OrganizationClosureAction.Reject:
                transition = closure.Reject(actor, command.Comment, clock.UtcNow);
                break;
            case OrganizationClosureAction.Schedule:
                transition = closure.Schedule(
                    actor,
                    command.ScheduledAtUtc ?? closure.RequestedEffectiveAtUtc
                );
                break;
            case OrganizationClosureAction.Cancel:
                transition = closure.Cancel(actor, command.Comment, clock.UtcNow);
                break;
            case OrganizationClosureAction.Complete:
                OrganizationClosureReadinessReport readiness = await readinessService.EvaluateAsync(
                    closure.OrganizationId,
                    cancellationToken
                );
                if (!readiness.CanClose)
                    return Result.Failure(OrganizationClosureErrors.ReadinessBlocked);

                OrganizationClosureExecutionResult execution = await orchestrator.ExecuteAsync(
                    closure,
                    actor,
                    cancellationToken
                );
                if (!execution.Succeeded)
                    return Result.Failure(OrganizationClosureErrors.OrchestrationFailed);

                transition = closure.Complete(actor, clock.UtcNow);
                break;
            default:
                return Result.Failure(OrganizationClosureErrors.InvalidAction);
        }

        if (transition.IsFailure)
            return transition;

        await unitOfWork.CommitAsync(cancellationToken);
        await auditSink.WriteAsync(
            $"OrganizationClosure{command.Action}",
            closure.OrganizationId,
            closure.Id,
            actor,
            new Dictionary<string, object?>
            {
                ["status"] = closure.Status.ToString(),
                ["revision"] = closure.Revision,
            },
            cancellationToken
        );
        return Result.Success();
    }
}
