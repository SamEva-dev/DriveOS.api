using DriveOS.Modules.Organizations.Domain.Subscriptions.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Subscriptions;

public sealed class OrganizationSubscription :
    AggregateRoot<OrganizationSubscriptionId>,
    IAuditableEntity
{
    private readonly List<SubscriptionEntitlement> _entitlements = [];
    private readonly List<SubscriptionLimit> _limits = [];

    private OrganizationSubscription()
    {
    }

    private OrganizationSubscription(
        OrganizationSubscriptionId id,
        OrganizationId organizationId,
        SubscriptionPlanCode planCode,
        SubscriptionStatus status,
        SubscriptionBillingCycle billingCycle,
        SubscriptionPeriod currentPeriod,
        SubscriptionPeriod? trialPeriod,
        string? externalProvider,
        string? externalSubscriptionId)
        : base(id)
    {
        OrganizationId = organizationId;
        PlanCode = planCode;
        Status = status;
        BillingCycle = billingCycle;
        CurrentPeriod = currentPeriod;
        TrialPeriod = trialPeriod;
        ExternalProvider = NormalizeOptional(externalProvider, 80);
        ExternalSubscriptionId = NormalizeOptional(externalSubscriptionId, 160);
        Version = 1;
    }

    public OrganizationId OrganizationId { get; private set; }
    public SubscriptionPlanCode PlanCode { get; private set; } = null!;
    public SubscriptionStatus Status { get; private set; }
    public SubscriptionBillingCycle BillingCycle { get; private set; }
    public SubscriptionPeriod CurrentPeriod { get; private set; } = null!;
    public SubscriptionPeriod? TrialPeriod { get; private set; }
    public SubscriptionCancellation? Cancellation { get; private set; }
    public string? ExternalProvider { get; private set; }
    public string? ExternalSubscriptionId { get; private set; }
    public int Version { get; private set; }

    public IReadOnlyCollection<SubscriptionEntitlement> Entitlements =>
        _entitlements.AsReadOnly();

    public IReadOnlyCollection<SubscriptionLimit> Limits =>
        _limits.AsReadOnly();

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<OrganizationSubscription> Create(
        OrganizationSubscriptionId id,
        OrganizationId organizationId,
        SubscriptionPlanCode planCode,
        SubscriptionStatus status,
        SubscriptionBillingCycle billingCycle,
        SubscriptionPeriod currentPeriod,
        SubscriptionPeriod? trialPeriod = null,
        string? externalProvider = null,
        string? externalSubscriptionId = null)
    {
        if (id.IsEmpty)
        {
            return Result.Failure<OrganizationSubscription>(
                OrganizationSubscriptionErrors.EmptyId);
        }

        if (organizationId.IsEmpty)
        {
            return Result.Failure<OrganizationSubscription>(
                OrganizationSubscriptionErrors.EmptyOrganizationId);
        }

        ArgumentNullException.ThrowIfNull(planCode);
        ArgumentNullException.ThrowIfNull(currentPeriod);

        if (!Enum.IsDefined(status) || !Enum.IsDefined(billingCycle))
        {
            return Result.Failure<OrganizationSubscription>(
                OrganizationSubscriptionErrors.InvalidStatusTransition);
        }

        if (status == SubscriptionStatus.Trialing && trialPeriod is null)
        {
            return Result.Failure<OrganizationSubscription>(
                OrganizationSubscriptionErrors.InvalidTrialPeriod);
        }

        if (trialPeriod is not null && trialPeriod.EndsAtUtc is null)
        {
            return Result.Failure<OrganizationSubscription>(
                OrganizationSubscriptionErrors.InvalidTrialPeriod);
        }

        var subscription = new OrganizationSubscription(
            id,
            organizationId,
            planCode,
            status,
            billingCycle,
            currentPeriod,
            trialPeriod,
            externalProvider,
            externalSubscriptionId);

        subscription.RaiseDomainEvent(
            new OrganizationSubscriptionCreatedDomainEvent(
                subscription.Id,
                subscription.OrganizationId,
                subscription.PlanCode.Value,
                subscription.Status));

        return Result.Success(subscription);
    }

    public Result ChangePlan(
        SubscriptionPlanCode newPlanCode,
        IReadOnlyCollection<string> entitlementCodes,
        IReadOnlyDictionary<string, long> limits,
        string? reason,
        UserId changedByUserId)
    {
        Result guard = GuardMutableChange(reason, changedByUserId);
        if (guard.IsFailure)
        {
            return guard;
        }

        ArgumentNullException.ThrowIfNull(newPlanCode);
        ArgumentNullException.ThrowIfNull(entitlementCodes);
        ArgumentNullException.ThrowIfNull(limits);

        Result<List<SubscriptionEntitlement>> entitlementsResult =
            BuildEntitlements(entitlementCodes);
        if (entitlementsResult.IsFailure)
        {
            return Result.Failure(entitlementsResult.Error);
        }

        Result<List<SubscriptionLimit>> limitsResult = BuildLimits(limits);
        if (limitsResult.IsFailure)
        {
            return Result.Failure(limitsResult.Error);
        }

        string previousPlanCode = PlanCode.Value;
        PlanCode = newPlanCode;
        ReplaceEntitlements(entitlementsResult.Value);
        ReplaceLimits(limitsResult.Value);
        IncrementVersion();

        RaiseDomainEvent(new OrganizationSubscriptionPlanChangedDomainEvent(
            Id,
            OrganizationId,
            previousPlanCode,
            PlanCode.Value,
            reason!.Trim(),
            changedByUserId));

        return Result.Success();
    }

    public Result Activate(
        SubscriptionPeriod currentPeriod,
        string? reason,
        UserId changedByUserId) =>
        ChangeStatus(
            SubscriptionStatus.Active,
            currentPeriod,
            reason,
            changedByUserId);

    public Result MarkPastDue(string? reason, UserId changedByUserId) =>
        ChangeStatus(SubscriptionStatus.PastDue, null, reason, changedByUserId);

    public Result Restrict(string? reason, UserId changedByUserId) =>
        ChangeStatus(SubscriptionStatus.Restricted, null, reason, changedByUserId);

    public Result Suspend(string? reason, UserId changedByUserId) =>
        ChangeStatus(SubscriptionStatus.Suspended, null, reason, changedByUserId);

    public Result Expire(string? reason, UserId changedByUserId) =>
        ChangeStatus(SubscriptionStatus.Expired, null, reason, changedByUserId);

    public Result Cancel(
        SubscriptionCancellation cancellation)
    {
        ArgumentNullException.ThrowIfNull(cancellation);

        if (Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Expired)
        {
            return Result.Failure(
                OrganizationSubscriptionErrors.InvalidStatusTransition);
        }

        SubscriptionStatus previousStatus = Status;
        Cancellation = cancellation;
        Status = SubscriptionStatus.Cancelled;
        IncrementVersion();

        RaiseDomainEvent(new OrganizationSubscriptionStatusChangedDomainEvent(
            Id,
            OrganizationId,
            previousStatus,
            Status,
            cancellation.Reason,
            cancellation.RequestedByUserId));

        return Result.Success();
    }

    public bool HasEntitlement(string entitlementCode) =>
        _entitlements.Any(item =>
            string.Equals(item.Code, entitlementCode?.Trim(), StringComparison.Ordinal));

    public long? GetLimit(string limitCode) =>
        _limits.FirstOrDefault(item =>
            string.Equals(item.Code, limitCode?.Trim(), StringComparison.Ordinal))?.Value;

    public void SetCreatedAudit(
    DateTimeOffset createdAtUtc,
    UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
        {
            return;
        }

        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(
        DateTimeOffset modifiedAtUtc,
        UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc;
        LastModifiedByUserId = modifiedByUserId;
    }

    private Result ChangeStatus(
        SubscriptionStatus targetStatus,
        SubscriptionPeriod? currentPeriod,
        string? reason,
        UserId changedByUserId)
    {
        Result guard = GuardMutableChange(reason, changedByUserId);
        if (guard.IsFailure)
        {
            return guard;
        }

        if (!CanTransition(Status, targetStatus))
        {
            return Result.Failure(
                OrganizationSubscriptionErrors.InvalidStatusTransition);
        }

        SubscriptionStatus previousStatus = Status;
        Status = targetStatus;

        if (currentPeriod is not null)
        {
            CurrentPeriod = currentPeriod;
        }

        IncrementVersion();

        RaiseDomainEvent(new OrganizationSubscriptionStatusChangedDomainEvent(
            Id,
            OrganizationId,
            previousStatus,
            Status,
            reason!.Trim(),
            changedByUserId));

        return Result.Success();
    }

    private Result GuardMutableChange(string? reason, UserId changedByUserId)
    {
        if (Status == SubscriptionStatus.Cancelled)
        {
            return Result.Failure(
                OrganizationSubscriptionErrors.CancelledSubscriptionCannotBeChanged);
        }

        if (changedByUserId.IsEmpty)
        {
            return Result.Failure(
                OrganizationSubscriptionErrors.EmptyActorUserId);
        }

        string normalizedReason = reason?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedReason))
        {
            return Result.Failure(
                OrganizationSubscriptionErrors.EmptyChangeReason);
        }

        if (normalizedReason.Length > SubscriptionCancellation.ReasonMaximumLength)
        {
            return Result.Failure(
                OrganizationSubscriptionErrors.ChangeReasonTooLong(
                    SubscriptionCancellation.ReasonMaximumLength));
        }

        return Result.Success();
    }

    private static bool CanTransition(
        SubscriptionStatus current,
        SubscriptionStatus target) =>
        current switch
        {
            SubscriptionStatus.Trialing => target is
                SubscriptionStatus.Active or
                SubscriptionStatus.Restricted or
                SubscriptionStatus.Suspended or
                SubscriptionStatus.Expired,

            SubscriptionStatus.Active => target is
                SubscriptionStatus.PastDue or
                SubscriptionStatus.Restricted or
                SubscriptionStatus.Suspended or
                SubscriptionStatus.Expired,

            SubscriptionStatus.PastDue => target is
                SubscriptionStatus.Active or
                SubscriptionStatus.Restricted or
                SubscriptionStatus.Suspended or
                SubscriptionStatus.Expired,

            SubscriptionStatus.Restricted => target is
                SubscriptionStatus.Active or
                SubscriptionStatus.PastDue or
                SubscriptionStatus.Suspended or
                SubscriptionStatus.Expired,

            SubscriptionStatus.Suspended => target is
                SubscriptionStatus.Active or
                SubscriptionStatus.Restricted or
                SubscriptionStatus.Expired,

            SubscriptionStatus.Expired => target is SubscriptionStatus.Active,
            SubscriptionStatus.Cancelled => false,
            _ => false,
        };

    private static Result<List<SubscriptionEntitlement>> BuildEntitlements(
        IEnumerable<string> codes)
    {
        var values = new List<SubscriptionEntitlement>();
        var uniqueCodes = new HashSet<string>(StringComparer.Ordinal);

        foreach (string code in codes)
        {
            Result<SubscriptionEntitlement> itemResult =
                SubscriptionEntitlement.Create(code);

            if (itemResult.IsFailure)
            {
                return Result.Failure<List<SubscriptionEntitlement>>(
                    itemResult.Error);
            }

            if (!uniqueCodes.Add(itemResult.Value.Code))
            {
                return Result.Failure<List<SubscriptionEntitlement>>(
                    OrganizationSubscriptionErrors.DuplicateEntitlement);
            }

            values.Add(itemResult.Value);
        }

        return Result.Success(values);
    }

    private static Result<List<SubscriptionLimit>> BuildLimits(
        IReadOnlyDictionary<string, long> limits)
    {
        var values = new List<SubscriptionLimit>();
        var uniqueCodes = new HashSet<string>(StringComparer.Ordinal);

        foreach ((string code, long value) in limits)
        {
            Result<SubscriptionLimit> itemResult =
                SubscriptionLimit.Create(code, value);

            if (itemResult.IsFailure)
            {
                return Result.Failure<List<SubscriptionLimit>>(itemResult.Error);
            }

            if (!uniqueCodes.Add(itemResult.Value.Code))
            {
                return Result.Failure<List<SubscriptionLimit>>(
                    OrganizationSubscriptionErrors.DuplicateLimit);
            }

            values.Add(itemResult.Value);
        }

        return Result.Success(values);
    }

    private void ReplaceEntitlements(IEnumerable<SubscriptionEntitlement> values)
    {
        _entitlements.Clear();
        _entitlements.AddRange(values);
    }

    private void ReplaceLimits(IEnumerable<SubscriptionLimit> values)
    {
        _limits.Clear();
        _limits.AddRange(values);
    }

    private void IncrementVersion() => Version++;

    private static string? NormalizeOptional(string? value, int maximumLength)
    {
        string? normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        return normalized is not null && normalized.Length > maximumLength
            ? normalized[..maximumLength]
            : normalized;
    }
}
