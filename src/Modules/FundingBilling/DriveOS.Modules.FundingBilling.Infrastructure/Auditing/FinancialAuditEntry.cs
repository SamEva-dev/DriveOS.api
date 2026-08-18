using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Auditing;

internal sealed class FinancialAuditEntry
{
    private FinancialAuditEntry() { }

    public Guid EventId { get; private set; }
    public OrganizationId OrganizationId { get; private set; }
    public BillingAccountId BillingAccountId { get; private set; }
    public string AggregateType { get; private set; } = string.Empty;
    public Guid AggregateId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public UserId? ActorUserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public string? DetailsJson { get; private set; }

    public static FinancialAuditEntry Create(
        Guid eventId,
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        string aggregateType,
        Guid aggregateId,
        string action,
        UserId? actorUserId,
        DateTimeOffset occurredAtUtc,
        string? detailsJson) => new()
        {
            EventId = eventId,
            OrganizationId = organizationId,
            BillingAccountId = billingAccountId,
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            Action = action,
            ActorUserId = actorUserId,
            OccurredAtUtc = occurredAtUtc.ToUniversalTime(),
            DetailsJson = detailsJson
        };
}
