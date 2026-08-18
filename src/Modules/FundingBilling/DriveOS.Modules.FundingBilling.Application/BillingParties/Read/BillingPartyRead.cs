using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.BillingParties.Read;

public sealed record BillingPartyResponse(Guid Id, Guid BillingAccountId, Guid? PersonId, Guid? OrganizationId, string Role, decimal? MaximumAmount, DateOnly EffectiveFrom, DateOnly? EffectiveTo, int Priority, bool IsPrimary, string Status, string? EndReason, DateTimeOffset? EndedAtUtc);
public interface IBillingPartyReadService { Task<IReadOnlyCollection<BillingPartyResponse>> ListAsync(OrganizationId organizationId, BillingAccountId billingAccountId, CancellationToken cancellationToken = default); }
public sealed record GetBillingPartiesQuery(OrganizationId OrganizationId, BillingAccountId BillingAccountId) : IQuery<IReadOnlyCollection<BillingPartyResponse>>;
internal sealed class GetBillingPartiesQueryHandler(IBillingPartyReadService readService) : IQueryHandler<GetBillingPartiesQuery, IReadOnlyCollection<BillingPartyResponse>>
{
    public async Task<Result<IReadOnlyCollection<BillingPartyResponse>>> Handle(GetBillingPartiesQuery query, CancellationToken cancellationToken) => Result.Success(await readService.ListAsync(query.OrganizationId, query.BillingAccountId, cancellationToken));
}
