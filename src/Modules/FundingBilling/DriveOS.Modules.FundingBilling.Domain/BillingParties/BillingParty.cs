using DriveOS.Modules.FundingBilling.Domain.BillingParties.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.BillingParties;

public sealed class BillingParty : AggregateRoot<BillingPartyId>, IAuditableEntity
{
    private BillingParty() { }
    private BillingParty(BillingPartyId id, OrganizationId organizationId, BillingAccountId billingAccountId, PersonId? personId, OrganizationId? partyOrganizationId, BillingPartyRole role, decimal? maximumAmount, DateOnly effectiveFrom, DateOnly? effectiveTo, int priority, bool isPrimary) : base(id)
    {
        OrganizationId = organizationId; BillingAccountId = billingAccountId; PersonId = personId; PartyOrganizationId = partyOrganizationId;
        Role = role; MaximumAmount = maximumAmount.HasValue ? Round(maximumAmount.Value) : null; EffectiveFrom = effectiveFrom; EffectiveTo = effectiveTo; Priority = priority; IsPrimary = isPrimary; Status = BillingPartyStatus.Active;
    }
    public OrganizationId OrganizationId { get; private set; }
    public BillingAccountId BillingAccountId { get; private set; }
    public PersonId? PersonId { get; private set; }
    public OrganizationId? PartyOrganizationId { get; private set; }
    public BillingPartyRole Role { get; private set; }
    public decimal? MaximumAmount { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public int Priority { get; private set; }
    public bool IsPrimary { get; private set; }
    public BillingPartyStatus Status { get; private set; }
    public string? EndReason { get; private set; }
    public DateTimeOffset? EndedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public bool CanPay => Role is BillingPartyRole.Payer or BillingPartyRole.PayerAndFunder;
    public bool CanFund => Role is BillingPartyRole.Funder or BillingPartyRole.PayerAndFunder;
    public bool IsEffectiveOn(DateOnly date) => Status == BillingPartyStatus.Active && date >= EffectiveFrom && (!EffectiveTo.HasValue || date <= EffectiveTo.Value);

    public static Result<BillingParty> Create(BillingPartyId id, OrganizationId organizationId, BillingAccountId billingAccountId, PersonId? personId, OrganizationId? partyOrganizationId, BillingPartyRole role, decimal? maximumAmount, DateOnly effectiveFrom, DateOnly? effectiveTo, int priority, bool isPrimary)
    {
        if (id.IsEmpty) return Result.Failure<BillingParty>(BillingPartyErrors.InvalidIdentifier);
        if (organizationId.IsEmpty || billingAccountId.IsEmpty) return Result.Failure<BillingParty>(BillingPartyErrors.InvalidOwner);
        if (personId.HasValue == partyOrganizationId.HasValue || personId.HasValue && personId.Value.IsEmpty || partyOrganizationId.HasValue && partyOrganizationId.Value.IsEmpty) return Result.Failure<BillingParty>(BillingPartyErrors.InvalidParty);
        if (!Enum.IsDefined(role)) return Result.Failure<BillingParty>(BillingPartyErrors.InvalidRole);
        if (effectiveFrom == default || effectiveTo.HasValue && effectiveTo.Value < effectiveFrom) return Result.Failure<BillingParty>(BillingPartyErrors.InvalidPeriod);
        if (maximumAmount.HasValue && Round(maximumAmount.Value) <= 0m) return Result.Failure<BillingParty>(BillingPartyErrors.InvalidAmount);
        if (priority is < 1 or > 100) return Result.Failure<BillingParty>(BillingPartyErrors.InvalidPriority);
        var value = new BillingParty(id, organizationId, billingAccountId, personId, partyOrganizationId, role, maximumAmount, effectiveFrom, effectiveTo, priority, isPrimary);
        value.RaiseDomainEvent(new BillingPartyAddedDomainEvent(value.Id, value.BillingAccountId, value.Role));
        return Result.Success(value);
    }

    public Result End(string reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status == BillingPartyStatus.Ended) return Result.Failure(BillingPartyErrors.AlreadyEnded);
        string normalized = reason?.Trim() ?? string.Empty;
        if (normalized.Length is < 3 or > 1000) return Result.Failure(BillingPartyErrors.InvalidReason);
        if (actorUserId.IsEmpty || occurredAtUtc == default) return Result.Failure(BillingPartyErrors.InvalidActor);
        Status = BillingPartyStatus.Ended; EndReason = normalized; EndedAtUtc = occurredAtUtc.ToUniversalTime();
        RaiseDomainEvent(new BillingPartyEndedDomainEvent(Id, EndReason, actorUserId, EndedAtUtc.Value));
        return Result.Success();
    }
    public void SetCreatedAudit(DateTimeOffset atUtc, UserId? userId) { if (CreatedAtUtc != default) return; CreatedAtUtc = atUtc.ToUniversalTime(); CreatedByUserId = userId; }
    public void SetModifiedAudit(DateTimeOffset atUtc, UserId? userId) { LastModifiedAtUtc = atUtc.ToUniversalTime(); LastModifiedByUserId = userId; }
    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
