using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.Modules.Organizations.Domain.BranchAssignments;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application
    .Branches.Lifecycle;

public sealed class ChangeBranchStatusCommandHandler(
    IBranchRepository branchRepository,
    IOrganizationRepository organizationRepository,
    IBranchUserAssignmentRepository branchUserAssignmentRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IClock clock)
    : ICommandHandler<ChangeBranchStatusCommand>
{
    public async Task<Result> Handle(
        ChangeBranchStatusCommand command,
        CancellationToken cancellationToken)
    {
        if (
            !currentUser.IsAuthenticated ||
            currentUser.UserId is null)
        {
            return Result.Failure(
                OrganizationErrors.CurrentUserRequired);
        }

        Organization? organization =
            await organizationRepository
                .GetByIdAsync(
                    command.OrganizationId,
                    asNoTracking: true,
                    cancellationToken);

        if (organization is null)
        {
            return Result.Failure(
                BranchErrors.OrganizationNotFound);
        }

        Branch? branch =
            await branchRepository
                .GetByIdAsync(
                    command.BranchId,
                    asNoTracking: false,
                    cancellationToken);

        if (
            branch is null ||
            branch.OrganizationId !=
            command.OrganizationId)
        {
            return Result.Failure(
                BranchErrors.NotFound);
        }

        BranchStatus currentStatus =
            branch.Status;

        if (
            command.TargetStatus ==
            BranchStatus.Active &&
            organization.Status !=
            OrganizationStatus.Active)
        {
            return Result.Failure(
                BranchErrors
                    .OrganizationMustBeActive);
        }

        BranchStatusChangeReason reason;

        try
        {
            reason =
                BranchStatusChangeReason.Create(
                    command.Reason);
        }
        catch (
            ArgumentException)
        {
            return Result.Failure(
                BranchErrors.InvalidStatusTransition(
                    currentStatus,
                    command.TargetStatus));
        }

        DateTimeOffset now = clock.UtcNow;

        // Compatibility bridge between the branch-team assignment model and
        // the historical primary-manager aggregate. In the current UI, the
        // "responsable d'agence" is created as a Primary AdministrativeManager
        // branch assignment. Activation, however, checks Branch.ManagerAssignments.
        // Keep the domain invariant and synchronize the aggregate before activation.
        if (
            command.TargetStatus == BranchStatus.Active &&
            currentStatus == BranchStatus.Draft &&
            !branch.HasActiveManagerAt(now))
        {
            IReadOnlyCollection<BranchUserAssignment> assignments =
                await branchUserAssignmentRepository.GetOpenAssignmentsByBranchAsync(
                    command.OrganizationId,
                    command.BranchId,
                    asNoTracking: true,
                    cancellationToken);

            BranchUserAssignment? primaryAdministrativeManager = assignments
                .Where(assignment =>
                    assignment.Status == BranchUserAssignmentStatus.Active &&
                    assignment.Role == BranchAssignmentRole.AdministrativeManager &&
                    assignment.AssignmentType == BranchAssignmentType.Primary &&
                    assignment.StartsAtUtc <= now &&
                    (!assignment.PlannedEndAtUtc.HasValue || assignment.PlannedEndAtUtc.Value > now) &&
                    (!assignment.EffectiveEndAtUtc.HasValue || assignment.EffectiveEndAtUtc.Value > now))
                .OrderByDescending(assignment => assignment.StartsAtUtc)
                .FirstOrDefault();

            if (primaryAdministrativeManager is not null)
            {
                Result managerResult = branch.AssignPrimaryManager(
                    primaryAdministrativeManager.UserId,
                    now,
                    currentUser.UserId.Value,
                    now);

                if (managerResult.IsFailure)
                {
                    return managerResult;
                }
            }
        }

        Result transitionResult =
            ApplyTransition(
                branch,
                command.TargetStatus,
                reason,
                currentUser.UserId.Value.Value,
                now,
                currentStatus);

        if (transitionResult.IsFailure)
        {
            return transitionResult;
        }

        await unitOfWork.CommitAsync(
            cancellationToken);

        return Result.Success();
    }

    private static Result ApplyTransition(
        Branch branch,
        BranchStatus targetStatus,
        BranchStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc,
        BranchStatus currentStatus)
    {
        try
        {
            switch (targetStatus)
            {
                case BranchStatus.Active:
                    {
                        if (
                            currentStatus ==
                            BranchStatus.Draft)
                        {
                            branch.Activate(
                                reason,
                                changedByUserId,
                                changedAtUtc);
                        }
                        else
                        {
                            branch.Reactivate(
                                reason,
                                changedByUserId,
                                changedAtUtc);
                        }

                        break;
                    }

                case BranchStatus.Restricted:
                    {
                        branch.Restrict(
                            reason,
                            changedByUserId,
                            changedAtUtc);

                        break;
                    }

                case BranchStatus.Suspended:
                    {
                        branch.Suspend(
                            reason,
                            changedByUserId,
                            changedAtUtc);

                        break;
                    }

                case BranchStatus.Closed:
                    {
                        branch.Close(
                            reason,
                            changedByUserId,
                            changedAtUtc);

                        break;
                    }

                default:
                    {
                        return Result.Failure(
                            BranchErrors
                                .InvalidStatusTransition(
                                    currentStatus,
                                    targetStatus));
                    }
            }

            return Result.Success();
        }
        catch (InvalidOperationException exception)
        {
            if (
                targetStatus ==
                    BranchStatus.Active &&
                currentStatus ==
                    BranchStatus.Draft &&
                exception.Message.Contains(
                    "manager",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure(
                    BranchErrors.ActiveManagerRequired);
            }

            return Result.Failure(
                BranchErrors
                    .InvalidStatusTransition(
                        currentStatus,
                        targetStatus));
        }
    }
}