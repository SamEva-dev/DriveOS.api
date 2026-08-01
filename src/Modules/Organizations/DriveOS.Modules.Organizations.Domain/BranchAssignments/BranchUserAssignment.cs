using DriveOS.Modules.Organizations.Domain
    .BranchAssignments.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain
    .BranchAssignments;

public sealed class BranchUserAssignment :
    AggregateRoot<BranchUserAssignmentId>,
    IAuditableEntity
{
    private BranchUserAssignment()
    {
    }

    private BranchUserAssignment(
        BranchUserAssignmentId id,
        OrganizationId organizationId,
        BranchId branchId,
        UserId userId,
        BranchAssignmentRole role,
        BranchAssignmentType assignmentType,
        DateTimeOffset startsAtUtc,
        DateTimeOffset? plannedEndAtUtc)
        : base(id)
    {
        OrganizationId =
            organizationId;

        BranchId =
            branchId;

        UserId =
            userId;

        Role =
            role;

        AssignmentType =
            assignmentType;

        StartsAtUtc =
            startsAtUtc;

        PlannedEndAtUtc =
            plannedEndAtUtc;

        Status =
            BranchUserAssignmentStatus.Active;
    }

    public OrganizationId OrganizationId
    {
        get;
        private set;
    }

    public BranchId BranchId
    {
        get;
        private set;
    }

    public UserId UserId
    {
        get;
        private set;
    }

    public BranchAssignmentRole Role
    {
        get;
        private set;
    }

    public BranchAssignmentType AssignmentType
    {
        get;
        private set;
    }

    public BranchUserAssignmentStatus Status
    {
        get;
        private set;
    }

    public DateTimeOffset StartsAtUtc
    {
        get;
        private set;
    }

    public DateTimeOffset? PlannedEndAtUtc
    {
        get;
        private set;
    }

    public DateTimeOffset? EffectiveEndAtUtc
    {
        get;
        private set;
    }

    public string? SuspensionReason
    {
        get;
        private set;
    }

    public DateTimeOffset? SuspendedAtUtc
    {
        get;
        private set;
    }

    public UserId? SuspendedByUserId
    {
        get;
        private set;
    }

    public string? EndReason
    {
        get;
        private set;
    }

    public DateTimeOffset? EndedAtUtc
    {
        get;
        private set;
    }

    public UserId? EndedByUserId
    {
        get;
        private set;
    }

    public DateTimeOffset CreatedAtUtc
    {
        get;
        private set;
    }

    public UserId? CreatedByUserId
    {
        get;
        private set;
    }

    public DateTimeOffset? LastModifiedAtUtc
    {
        get;
        private set;
    }

    public UserId? LastModifiedByUserId
    {
        get;
        private set;
    }

    public static Result<BranchUserAssignment>
        Create(
            BranchUserAssignmentId id,
            OrganizationId organizationId,
            BranchId branchId,
            UserId userId,
            BranchAssignmentRole role,
            BranchAssignmentType assignmentType,
            DateTimeOffset startsAtUtc,
            DateTimeOffset? plannedEndAtUtc,
            UserId assignedByUserId,
            DateTimeOffset assignedAtUtc)
    {
        if (id.IsEmpty)
        {
            return Result.Failure<
                BranchUserAssignment>(
                    BranchUserAssignmentErrors
                        .EmptyId);
        }

        if (organizationId.IsEmpty)
        {
            return Result.Failure<
                BranchUserAssignment>(
                    BranchUserAssignmentErrors
                        .EmptyOrganizationId);
        }

        if (branchId.IsEmpty)
        {
            return Result.Failure<
                BranchUserAssignment>(
                    BranchUserAssignmentErrors
                        .EmptyBranchId);
        }

        if (userId.IsEmpty)
        {
            return Result.Failure<
                BranchUserAssignment>(
                    BranchUserAssignmentErrors
                        .EmptyUserId);
        }

        if (assignedByUserId.IsEmpty)
        {
            return Result.Failure<
                BranchUserAssignment>(
                    BranchUserAssignmentErrors
                        .EmptyCreatedByUserId);
        }

        if (!Enum.IsDefined(role))
        {
            return Result.Failure<
                BranchUserAssignment>(
                    BranchUserAssignmentErrors
                        .InvalidRole);
        }

        if (!Enum.IsDefined(assignmentType))
        {
            return Result.Failure<
                BranchUserAssignment>(
                    BranchUserAssignmentErrors
                        .InvalidType);
        }

        if (
            startsAtUtc == default ||
            assignedAtUtc == default)
        {
            return Result.Failure<
                BranchUserAssignment>(
                    BranchUserAssignmentErrors
                        .InvalidStartDate);
        }

        if (startsAtUtc != assignedAtUtc)
        {
            return Result.Failure<
                BranchUserAssignment>(
                    BranchUserAssignmentErrors
                        .InvalidStartDate);
        }

        if (
            plannedEndAtUtc.HasValue &&
            plannedEndAtUtc.Value <=
            startsAtUtc)
        {
            return Result.Failure<
                BranchUserAssignment>(
                    BranchUserAssignmentErrors
                        .InvalidEndDate);
        }

        var assignment =
            new BranchUserAssignment(
                id,
                organizationId,
                branchId,
                userId,
                role,
                assignmentType,
                startsAtUtc,
                plannedEndAtUtc);

        assignment.RaiseDomainEvent(
            new BranchUserAssignedDomainEvent(
                assignment.Id,
                assignment.OrganizationId,
                assignment.BranchId,
                assignment.UserId,
                assignment.Role,
                assignment.AssignmentType,
                assignment.StartsAtUtc,
                assignment.PlannedEndAtUtc,
                assignedByUserId,
                assignedAtUtc));

        return Result.Success(
            assignment);
    }

    public Result Suspend(
        BranchAssignmentReason reason,
        UserId suspendedByUserId,
        DateTimeOffset suspendedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(
            reason);

        if (
            Status ==
            BranchUserAssignmentStatus.Ended)
        {
            return Result.Failure(
                BranchUserAssignmentErrors
                    .CannotModifyEnded);
        }

        if (
            Status ==
            BranchUserAssignmentStatus.Suspended)
        {
            return Result.Failure(
                BranchUserAssignmentErrors
                    .AlreadySuspended);
        }

        if (suspendedByUserId.IsEmpty)
        {
            return Result.Failure(
                BranchUserAssignmentErrors
                    .EmptyChangedByUserId);
        }

        Status =
            BranchUserAssignmentStatus.Suspended;

        SuspensionReason =
            reason.Value;

        SuspendedAtUtc =
            suspendedAtUtc;

        SuspendedByUserId =
            suspendedByUserId;

        RaiseDomainEvent(
            new BranchUserAssignmentSuspendedDomainEvent(
                Id,
                OrganizationId,
                BranchId,
                UserId,
                reason.Value,
                suspendedByUserId,
                suspendedAtUtc));

        return Result.Success();
    }

    public Result Reactivate(
        BranchAssignmentReason reason,
        UserId reactivatedByUserId,
        DateTimeOffset reactivatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(
            reason);

        if (
            Status ==
            BranchUserAssignmentStatus.Ended)
        {
            return Result.Failure(
                BranchUserAssignmentErrors
                    .CannotModifyEnded);
        }

        if (
            Status !=
            BranchUserAssignmentStatus.Suspended)
        {
            return Result.Failure(
                BranchUserAssignmentErrors
                    .NotSuspended);
        }

        if (reactivatedByUserId.IsEmpty)
        {
            return Result.Failure(
                BranchUserAssignmentErrors
                    .EmptyChangedByUserId);
        }

        Status =
            BranchUserAssignmentStatus.Active;

        SuspensionReason =
            null;

        SuspendedAtUtc =
            null;

        SuspendedByUserId =
            null;

        RaiseDomainEvent(
            new BranchUserAssignmentReactivatedDomainEvent(
                Id,
                OrganizationId,
                BranchId,
                UserId,
                reason.Value,
                reactivatedByUserId,
                reactivatedAtUtc));

        return Result.Success();
    }

    public Result End(
        BranchAssignmentReason reason,
        DateTimeOffset effectiveEndAtUtc,
        UserId endedByUserId,
        DateTimeOffset endedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(
            reason);

        if (
            Status ==
            BranchUserAssignmentStatus.Ended)
        {
            return Result.Failure(
                BranchUserAssignmentErrors
                    .AlreadyEnded);
        }

        if (endedByUserId.IsEmpty)
        {
            return Result.Failure(
                BranchUserAssignmentErrors
                    .EmptyChangedByUserId);
        }

        if (
            effectiveEndAtUtc <
            StartsAtUtc)
        {
            return Result.Failure(
                BranchUserAssignmentErrors
                    .InvalidEndDate);
        }

        if (
            effectiveEndAtUtc >
            endedAtUtc)
        {
            return Result.Failure(
                BranchUserAssignmentErrors
                    .InvalidEndDate);
        }

        Status =
            BranchUserAssignmentStatus.Ended;

        EffectiveEndAtUtc =
            effectiveEndAtUtc;

        EndReason =
            reason.Value;

        EndedAtUtc =
            endedAtUtc;

        EndedByUserId =
            endedByUserId;

        SuspensionReason =
            null;

        SuspendedAtUtc =
            null;

        SuspendedByUserId =
            null;

        RaiseDomainEvent(
            new BranchUserAssignmentEndedDomainEvent(
                Id,
                OrganizationId,
                BranchId,
                UserId,
                reason.Value,
                effectiveEndAtUtc,
                endedByUserId,
                endedAtUtc));

        return Result.Success();
    }

    public bool IsEffectiveAt(
        DateTimeOffset dateTimeUtc)
    {
        return
            Status ==
                BranchUserAssignmentStatus.Active &&
            StartsAtUtc <= dateTimeUtc &&
            (
                EffectiveEndAtUtc is null ||
                EffectiveEndAtUtc >
                    dateTimeUtc
            ) &&
            (
                PlannedEndAtUtc is null ||
                PlannedEndAtUtc >
                    dateTimeUtc
            );
    }

    public void SetCreatedAudit(
    DateTimeOffset createdAtUtc,
    UserId? createdByUserId)
    {
        CreatedAtUtc =
            createdAtUtc;

        CreatedByUserId =
            createdByUserId;
    }

    public void SetModifiedAudit(
        DateTimeOffset modifiedAtUtc,
        UserId? modifiedByUserId)
    {
        LastModifiedAtUtc =
            modifiedAtUtc;

        LastModifiedByUserId =
            modifiedByUserId;
    }
}