using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Read;

public enum TrainingContractSortField
{
    CreatedAt = 0,
    ContractNumber = 1,
    StartDate = 2,
    EndDate = 3,
    Status = 4,
    TotalAmount = 5,
}

public sealed record SearchTrainingContractsQuery(
    OrganizationId OrganizationId,
    int PageNumber,
    int PageSize,
    string? Search,
    PersonId? StudentId,
    BranchId? BranchId,
    TrainingContractStatus? Status,
    DateOnly? StartsFrom,
    DateOnly? StartsTo,
    DateOnly? EndsFrom,
    DateOnly? EndsTo,
    TrainingContractSortField SortBy,
    SortDirection SortDirection)
    : IQuery<PagedResult<TrainingContractListItemResponse>>;
