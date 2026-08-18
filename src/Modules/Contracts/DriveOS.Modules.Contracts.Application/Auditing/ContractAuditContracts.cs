using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Application.Auditing;

public sealed record ContractAuditEntryResponse(
    Guid EventId,
    Guid ContractId,
    string AggregateType,
    Guid AggregateId,
    string Action,
    Guid? ActorUserId,
    DateTimeOffset OccurredAtUtc,
    string? DetailsJson);

public sealed record GetContractAuditQuery(
    OrganizationId OrganizationId,
    TrainingContractId ContractId)
    : IQuery<IReadOnlyList<ContractAuditEntryResponse>>;

public interface IContractAuditReadService
{
    Task<IReadOnlyList<ContractAuditEntryResponse>> ListAsync(
        OrganizationId organizationId,
        TrainingContractId contractId,
        CancellationToken cancellationToken = default);
}
