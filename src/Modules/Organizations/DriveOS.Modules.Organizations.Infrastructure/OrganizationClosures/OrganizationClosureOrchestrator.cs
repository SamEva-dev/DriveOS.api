using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Commands;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationClosures;

internal sealed class OrganizationClosureOrchestrator(
    OrganizationsDbContext dbContext,
    IOrganizationArchiveService archiveService,
    IOrganizationAnonymizationService anonymizationService,
    IOrganizationClosureAuditSink auditSink,
    IClock clock,
    ILogger<OrganizationClosureOrchestrator> logger
) : IOrganizationClosureOrchestrator
{
    public async Task<OrganizationClosureExecutionResult> ExecuteAsync(
        OrganizationClosure closure,
        UserId actorUserId,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(closure);

        var steps = new List<OrganizationClosureStepResult>();
        DateTimeOffset now = clock.UtcNow;

        await RunAsync(OrganizationClosureSteps.BlockNewOperations, NoOpAsync, steps);
        await RunAsync(OrganizationClosureSteps.CloseBranches, CloseBranchesAsync, steps);
        await RunAsync(OrganizationClosureSteps.FreezeSequences, FreezeSequencesAsync, steps);
        await RunAsync(OrganizationClosureSteps.EndRepresentatives, EndRepresentativesAsync, steps);
        await RunAsync(OrganizationClosureSteps.RevokeAccess, NoOpAsync, steps);
        await RunAsync(
            OrganizationClosureSteps.TerminateSubscription,
            TerminateSubscriptionAsync,
            steps
        );
        await RunAsync(OrganizationClosureSteps.DisableIntegrations, NoOpAsync, steps);
        await RunAsync(OrganizationClosureSteps.ArchiveData, ArchiveAsync, steps);
        await RunAsync(
            OrganizationClosureSteps.FinalizeOrganization,
            FinalizeOrganizationAsync,
            steps
        );

        bool succeeded = steps.All(x => x.Succeeded);
        await auditSink.WriteAsync(
            succeeded
                ? "OrganizationClosureExecutionSucceeded"
                : "OrganizationClosureExecutionFailed",
            closure.OrganizationId,
            closure.Id,
            actorUserId,
            new Dictionary<string, object?>
            {
                ["steps"] = steps
                    .Select(x => new
                    {
                        x.Step,
                        x.Succeeded,
                        x.ErrorCode,
                    })
                    .ToArray(),
                ["occurredAtUtc"] = now,
            },
            cancellationToken
        );

        return new OrganizationClosureExecutionResult(succeeded, steps);

        Task NoOpAsync() => Task.CompletedTask;

        async Task CloseBranchesAsync()
        {
            List<Branch> branches = await dbContext
                .Branches.Where(x =>
                    x.OrganizationId == closure.OrganizationId && x.Status != BranchStatus.Closed
                )
                .ToListAsync(cancellationToken);

            var reason = BranchStatusChangeReason.Create("Organization closure execution.");
            foreach (Branch branch in branches)
            {
                branch.Close(reason, actorUserId.Value, now);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        async Task FreezeSequencesAsync()
        {
            List<OrganizationSequence> sequences = await dbContext
                .OrganizationSequences.Where(x =>
                    x.OrganizationId == closure.OrganizationId
                    && x.Status != OrganizationSequenceStatus.Archived
                )
                .ToListAsync(cancellationToken);

            foreach (OrganizationSequence sequence in sequences)
            {
                var result = sequence.Archive();
                if (result.IsFailure)
                    throw new InvalidOperationException(result.Error.Code);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        async Task EndRepresentativesAsync()
        {
            List<OrganizationRepresentative> representatives = await dbContext
                .OrganizationRepresentatives.Where(x =>
                    x.OrganizationId == closure.OrganizationId
                    && x.Status == OrganizationRepresentativeStatus.Active
                )
                .ToListAsync(cancellationToken);

            // Suspension is used during organizational closure because the domain invariant
            // intentionally forbids ending the last active owner.
            foreach (OrganizationRepresentative representative in representatives)
            {
                var result = representative.Suspend("Organization closure execution.", actorUserId);
                if (result.IsFailure)
                    throw new InvalidOperationException(result.Error.Code);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        async Task TerminateSubscriptionAsync()
        {
            OrganizationSubscription? subscription =
                await dbContext.OrganizationSubscriptions.SingleOrDefaultAsync(
                    x => x.OrganizationId == closure.OrganizationId,
                    cancellationToken
                );

            if (
                subscription is null
                || subscription.Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Expired
            )
                return;

            var cancellationResult = SubscriptionCancellation.Create(
                now,
                now,
                "Organization closure execution.",
                actorUserId
            );

            if (cancellationResult.IsFailure)
                throw new InvalidOperationException(cancellationResult.Error.Code);

            var result = subscription.Cancel(cancellationResult.Value);
            if (result.IsFailure)
                throw new InvalidOperationException(result.Error.Code);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        Task ArchiveAsync() => archiveService.ArchiveAsync(closure, cancellationToken);

        async Task FinalizeOrganizationAsync()
        {
            Organization? organization = await dbContext.Organizations.SingleOrDefaultAsync(
                x => x.Id == closure.OrganizationId,
                cancellationToken
            );

            if (organization is null)
                throw new InvalidOperationException("Organizations.NotFound");

            if (organization.Status != OrganizationStatus.Closed)
            {
                organization.Close(
                    OrganizationStatusChangeReason.Create("Organization closure completed."),
                    actorUserId.Value,
                    now
                );
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        async Task RunAsync(
            string step,
            Func<Task> action,
            ICollection<OrganizationClosureStepResult> results
        )
        {
            try
            {
                await action();
                results.Add(new OrganizationClosureStepResult(step, true));
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Organization closure step {Step} failed for {OrganizationId}.",
                    step,
                    closure.OrganizationId
                );
                results.Add(new OrganizationClosureStepResult(step, false, exception.Message));
            }
        }
    }

    public async Task<OrganizationClosureExecutionResult> ReopenAsync(
        OrganizationId organizationId,
        string justification,
        UserId actorUserId,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(justification))
            throw new ArgumentException(
                "A reopening justification is required.",
                nameof(justification)
            );

        if (
            await anonymizationService.HasIrreversibleAnonymizationStartedAsync(
                organizationId,
                cancellationToken
            )
        )
        {
            return new OrganizationClosureExecutionResult(
                false,
                [
                    new OrganizationClosureStepResult(
                        "verify-anonymization",
                        false,
                        "Organizations.Closure.IrreversibleAnonymizationStarted"
                    ),
                ]
            );
        }

        // Reopening must pass the normal activation-readiness command flow. This method
        // deliberately does not force an Active status or recreate subscriptions/accesses.
        logger.LogInformation(
            "Organization {OrganizationId} reopening requested by {ActorUserId}. Justification: {Justification}",
            organizationId,
            actorUserId,
            justification.Trim()
        );

        return new OrganizationClosureExecutionResult(
            true,
            [new OrganizationClosureStepResult("reopening-request-accepted", true)]
        );
    }
}
