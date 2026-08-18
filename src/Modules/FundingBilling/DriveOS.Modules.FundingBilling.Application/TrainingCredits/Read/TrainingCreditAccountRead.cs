using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.Modules.FundingBilling.Domain.TrainingCredits;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.TrainingCredits.Read;

public sealed record TrainingCreditAccountResponse(
    Guid Id,
    Guid BillingAccountId,
    string CreditType,
    decimal QuantityPurchased,
    decimal QuantityReserved,
    decimal QuantityConsumed,
    decimal Adjustments,
    decimal QuantityAvailable,
    DateOnly? ExpirationDate,
    string Status);

public sealed record TrainingCreditMovementResponse(Guid Id, string Type, decimal Quantity, string Reference, string? Reason, DateTimeOffset OccurredAtUtc, Guid ActorUserId);

public interface ITrainingCreditAccountReadService
{
    Task<IReadOnlyCollection<TrainingCreditAccountResponse>> ListAsync(OrganizationId organizationId, BillingAccountId billingAccountId, CancellationToken cancellationToken = default);
    Task<TrainingCreditAccountResponse?> GetAsync(OrganizationId organizationId, TrainingCreditAccountId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TrainingCreditMovementResponse>> ListMovementsAsync(OrganizationId organizationId, TrainingCreditAccountId id, CancellationToken cancellationToken = default);
}

public sealed record GetTrainingCreditAccountsQuery(OrganizationId OrganizationId, BillingAccountId BillingAccountId) : IQuery<IReadOnlyCollection<TrainingCreditAccountResponse>>;
public sealed record GetTrainingCreditAccountQuery(OrganizationId OrganizationId, TrainingCreditAccountId TrainingCreditAccountId) : IQuery<TrainingCreditAccountResponse>;

internal sealed class GetTrainingCreditAccountsQueryHandler(ITrainingCreditAccountReadService readService) : IQueryHandler<GetTrainingCreditAccountsQuery, IReadOnlyCollection<TrainingCreditAccountResponse>>
{
    public async Task<Result<IReadOnlyCollection<TrainingCreditAccountResponse>>> Handle(GetTrainingCreditAccountsQuery query, CancellationToken cancellationToken) =>
        Result.Success(await readService.ListAsync(query.OrganizationId, query.BillingAccountId, cancellationToken));
}

internal sealed class GetTrainingCreditAccountQueryHandler(ITrainingCreditAccountReadService readService) : IQueryHandler<GetTrainingCreditAccountQuery, TrainingCreditAccountResponse>
{
    public async Task<Result<TrainingCreditAccountResponse>> Handle(GetTrainingCreditAccountQuery query, CancellationToken cancellationToken)
    {
        TrainingCreditAccountResponse? account = await readService.GetAsync(query.OrganizationId, query.TrainingCreditAccountId, cancellationToken);
        return account is null ? Result.Failure<TrainingCreditAccountResponse>(TrainingCreditAccountErrors.NotFound) : Result.Success(account);
    }
}


public sealed record GetTrainingCreditMovementsQuery(OrganizationId OrganizationId, TrainingCreditAccountId TrainingCreditAccountId) : IQuery<IReadOnlyCollection<TrainingCreditMovementResponse>>;

internal sealed class GetTrainingCreditMovementsQueryHandler(ITrainingCreditAccountReadService readService) : IQueryHandler<GetTrainingCreditMovementsQuery, IReadOnlyCollection<TrainingCreditMovementResponse>>
{
    public async Task<Result<IReadOnlyCollection<TrainingCreditMovementResponse>>> Handle(GetTrainingCreditMovementsQuery query, CancellationToken cancellationToken) =>
        Result.Success(await readService.ListMovementsAsync(query.OrganizationId, query.TrainingCreditAccountId, cancellationToken));
}
