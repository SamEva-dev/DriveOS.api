using DriveOS.Modules.Organizations.Domain.Branches.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Branches;

public sealed class Branch : AggregateRoot<BranchId>, IAuditableEntity
{
    private readonly List<BranchStatusHistoryEntry> _statusHistory = [];
    private readonly List<BranchManagerAssignment> _managerAssignments = [];

    private Branch() { }

    private Branch(
        BranchId id,
        OrganizationId organizationId,
        BranchName name,
        BranchCode code,
        BranchType type,
        BranchAddress address,
        string timeZoneId,
        bool isPrimary
    )
        : base(id)
    {
        OrganizationId = organizationId;
        Name = name;
        NormalizedName = name.NormalizedValue;
        Code = code;
        Type = type;
        Address = address;
        TimeZoneId = timeZoneId;
        IsPrimary = isPrimary;
        Status = BranchStatus.Draft;
    }

    public OrganizationId OrganizationId { get; private set; }

    public BranchName Name { get; private set; } = null!;

    public string NormalizedName { get; private set; } = string.Empty;

    public BranchCode Code { get; private set; } = null!;

    public BranchType Type { get; private set; }

    public BranchAddress Address { get; private set; } = null!;

    public string TimeZoneId { get; private set; } = string.Empty;

    public bool IsPrimary { get; private set; }

    public BranchStatus Status { get; private set; }

    public IReadOnlyCollection<BranchStatusHistoryEntry> StatusHistory =>
        _statusHistory.AsReadOnly();

    public IReadOnlyCollection<BranchManagerAssignment> ManagerAssignments =>
        _managerAssignments.AsReadOnly();

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public UserId? CreatedByUserId { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<Branch> Create(
        BranchId id,
        OrganizationId organizationId,
        BranchName name,
        BranchCode code,
        BranchType type,
        BranchAddress address,
        string? timeZoneId,
        bool isPrimary
    )
    {
        if (id.IsEmpty)
        {
            return Result.Failure<Branch>(BranchErrors.EmptyId);
        }

        if (organizationId.IsEmpty)
        {
            return Result.Failure<Branch>(BranchErrors.EmptyOrganizationId);
        }

        ArgumentNullException.ThrowIfNull(name);

        ArgumentNullException.ThrowIfNull(code);

        ArgumentNullException.ThrowIfNull(address);

        if (!Enum.IsDefined(type))
        {
            return Result.Failure<Branch>(BranchErrors.InvalidBranchType);
        }

        string normalizedTimeZoneId = timeZoneId?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedTimeZoneId) || normalizedTimeZoneId.Length > 100)
        {
            return Result.Failure<Branch>(BranchErrors.InvalidTimeZone);
        }

        var branch = new Branch(
            id,
            organizationId,
            name,
            code,
            type,
            address,
            normalizedTimeZoneId,
            isPrimary
        );

        branch.RaiseDomainEvent(
            new BranchCreatedDomainEvent(
                branch.Id,
                branch.OrganizationId,
                branch.Name.Value,
                branch.Code.Value,
                branch.Type,
                branch.IsPrimary
            )
        );

        return Result.Success(branch);
    }

    public Result UpdateGeneralInformation(
        BranchName name,
        BranchType type,
        BranchAddress address,
        string? timeZoneId
    )
    {
        if (Status == BranchStatus.Closed)
        {
            return Result.Failure(BranchErrors.ClosedBranchCannotBeModified);
        }

        ArgumentNullException.ThrowIfNull(name);

        ArgumentNullException.ThrowIfNull(address);

        if (!Enum.IsDefined(type))
        {
            return Result.Failure(BranchErrors.InvalidBranchType);
        }

        string normalizedTimeZoneId = timeZoneId?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedTimeZoneId) || normalizedTimeZoneId.Length > 100)
        {
            return Result.Failure(BranchErrors.InvalidTimeZone);
        }

        string previousName = Name.Value;

        BranchType previousType = Type;

        BranchAddress previousAddress = Address;

        string previousTimeZoneId = TimeZoneId;

        Name = name;
        NormalizedName = name.NormalizedValue;
        Type = type;
        Address = address;
        TimeZoneId = normalizedTimeZoneId;

        RaiseDomainEvent(
            new BranchUpdatedDomainEvent(
                Id,
                OrganizationId,
                previousName,
                Name.Value,
                previousType,
                Type,
                previousAddress,
                Address,
                previousTimeZoneId,
                TimeZoneId
            )
        );

        return Result.Success();
    }

    public Result SetAsPrimary()
    {
        if (Status == BranchStatus.Closed)
        {
            return Result.Failure(BranchErrors.ClosedBranchCannotBePrimary);
        }

        if (IsPrimary)
        {
            return Result.Success();
        }

        IsPrimary = true;

        RaiseDomainEvent(new BranchSetAsPrimaryDomainEvent(Id, OrganizationId));

        return Result.Success();
    }

    public void RemovePrimaryDesignation()
    {
        if (!IsPrimary)
        {
            return;
        }

        IsPrimary = false;

        RaiseDomainEvent(new BranchPrimaryDesignationRemovedDomainEvent(Id, OrganizationId));
    }

    public void Activate(
        BranchStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc
    )
    {
        EnsureStatus(BranchStatus.Draft, "Only a draft branch can be activated.");

        if (!HasActiveManagerAt(changedAtUtc))
        {
            throw new InvalidOperationException(
                "An active branch manager is required before branch activation."
            );
        }

        ChangeStatus(BranchStatus.Active, reason, changedByUserId, changedAtUtc);
    }

    public void Restrict(
        BranchStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc
    )
    {
        EnsureStatus(BranchStatus.Active, "Only an active branch can be restricted.");

        ChangeStatus(BranchStatus.Restricted, reason, changedByUserId, changedAtUtc);
    }

    public void Suspend(
        BranchStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc
    )
    {
        EnsureStatusIn(
            [BranchStatus.Active, BranchStatus.Restricted],
            "Only an active or restricted branch can be suspended."
        );

        ChangeStatus(BranchStatus.Suspended, reason, changedByUserId, changedAtUtc);
    }

    public void Reactivate(
        BranchStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc
    )
    {
        EnsureStatusIn(
            [BranchStatus.Restricted, BranchStatus.Suspended],
            "Only a restricted or suspended branch can be reactivated."
        );

        ChangeStatus(BranchStatus.Active, reason, changedByUserId, changedAtUtc);
    }

    public void Close(
        BranchStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc
    )
    {
        EnsureStatusIn(
            [BranchStatus.Active, BranchStatus.Restricted, BranchStatus.Suspended],
            "The branch cannot be closed from its current status."
        );

        var changedBy = new UserId(changedByUserId);

        EndActiveManagerAssignment(changedAtUtc, changedBy, changedAtUtc);

        if (IsPrimary)
        {
            RemovePrimaryDesignation();
        }

        ChangeStatus(BranchStatus.Closed, reason, changedByUserId, changedAtUtc);
    }

    public Result AssignPrimaryManager(
        UserId managerUserId,
        DateTimeOffset effectiveFromUtc,
        UserId assignedByUserId,
        DateTimeOffset assignedAtUtc
    )
    {
        if (Status == BranchStatus.Closed)
        {
            return Result.Failure(BranchErrors.ClosedBranchCannotReceiveManager);
        }

        if (managerUserId.IsEmpty)
        {
            return Result.Failure(BranchErrors.EmptyManagerUserId);
        }

        if (assignedByUserId.IsEmpty)
        {
            return Result.Failure(BranchErrors.EmptyAssignedByUserId);
        }

        if (effectiveFromUtc == default)
        {
            return Result.Failure(BranchErrors.ManagerEffectiveDateInvalid);
        }

        if (effectiveFromUtc < assignedAtUtc)
        {
            return Result.Failure(BranchErrors.ManagerEffectiveDateCannotBePast);
        }

        if (effectiveFromUtc > assignedAtUtc)
        {
            return Result.Failure(BranchErrors.ManagerEffectiveDateCannotBeFuture);
        }

        BranchManagerAssignment? currentAssignment = GetActiveManagerAssignmentAt(effectiveFromUtc);

        if (currentAssignment is not null && currentAssignment.ManagerUserId == managerUserId)
        {
            return Result.Success();
        }

        if (currentAssignment is not null)
        {
            EndActiveManagerAssignment(effectiveFromUtc, assignedByUserId, assignedAtUtc);
        }

        BranchManagerAssignment newAssignment = BranchManagerAssignment.Create(
            Id,
            managerUserId,
            effectiveFromUtc,
            assignedByUserId,
            assignedAtUtc
        );

        _managerAssignments.Add(newAssignment);

        RaiseDomainEvent(
            new BranchManagerAssignedDomainEvent(
                Id,
                OrganizationId,
                newAssignment.Id,
                managerUserId,
                effectiveFromUtc,
                assignedByUserId,
                assignedAtUtc
            )
        );

        return Result.Success();
    }

    public BranchManagerAssignment? GetActiveManagerAssignmentAt(DateTimeOffset dateTimeUtc)
    {
        return _managerAssignments
            .Where(assignment => assignment.IsActiveAt(dateTimeUtc))
            .OrderByDescending(assignment => assignment.EffectiveFromUtc)
            .FirstOrDefault();
    }

    public bool HasActiveManagerAt(DateTimeOffset dateTimeUtc)
    {
        return GetActiveManagerAssignmentAt(dateTimeUtc) is not null;
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
        {
            return;
        }

        CreatedAtUtc = createdAtUtc;

        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc;

        LastModifiedByUserId = modifiedByUserId;
    }

    private void EndActiveManagerAssignment(
        DateTimeOffset effectiveToUtc,
        UserId endedByUserId,
        DateTimeOffset endedAtUtc
    )
    {
        BranchManagerAssignment? currentAssignment = GetActiveManagerAssignmentAt(effectiveToUtc);

        if (currentAssignment is null)
        {
            return;
        }

        currentAssignment.End(effectiveToUtc, endedByUserId, endedAtUtc);

        RaiseDomainEvent(
            new BranchManagerAssignmentEndedDomainEvent(
                Id,
                OrganizationId,
                currentAssignment.Id,
                currentAssignment.ManagerUserId,
                effectiveToUtc,
                endedByUserId,
                endedAtUtc
            )
        );
    }

    private void ChangeStatus(
        BranchStatus newStatus,
        BranchStatusChangeReason reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc
    )
    {
        ArgumentNullException.ThrowIfNull(reason);

        BranchStatus previousStatus = Status;

        Status = newStatus;

        _statusHistory.Add(
            BranchStatusHistoryEntry.Create(
                Id,
                previousStatus,
                newStatus,
                reason,
                changedByUserId,
                changedAtUtc
            )
        );

        RaiseDomainEvent(
            new BranchStatusChangedDomainEvent(
                Id,
                OrganizationId,
                previousStatus,
                newStatus,
                reason.Value,
                changedByUserId,
                changedAtUtc
            )
        );
    }

    private void EnsureStatus(BranchStatus expectedStatus, string errorMessage)
    {
        if (Status != expectedStatus)
        {
            throw new InvalidOperationException(errorMessage);
        }
    }

    private void EnsureStatusIn(
        IReadOnlyCollection<BranchStatus> allowedStatuses,
        string errorMessage
    )
    {
        if (!allowedStatuses.Contains(Status))
        {
            throw new InvalidOperationException(errorMessage);
        }
    }
}
