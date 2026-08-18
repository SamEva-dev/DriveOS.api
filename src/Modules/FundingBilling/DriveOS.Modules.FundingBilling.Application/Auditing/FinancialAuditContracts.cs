using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Application.Auditing;

public sealed record FinancialAuditEntryResponse(
    Guid EventId,
    Guid BillingAccountId,
    string AggregateType,
    Guid AggregateId,
    string Action,
    Guid? ActorUserId,
    DateTimeOffset OccurredAtUtc,
    string? DetailsJson);

public sealed record GetFinancialAuditQuery(
    OrganizationId OrganizationId,
    BillingAccountId BillingAccountId)
    : IQuery<IReadOnlyList<FinancialAuditEntryResponse>>;

public interface IFinancialAuditReadService
{
    Task<IReadOnlyList<FinancialAuditEntryResponse>> ListAsync(
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        CancellationToken cancellationToken = default);
}
