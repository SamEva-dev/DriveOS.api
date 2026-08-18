using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Read;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Contracts.TrainingContracts;

public sealed class TrainingContractReadHandlerTests
{
    [Fact]
    public async Task Search_ShouldForwardTenantFiltersPaginationAndSortToReadService()
    {
        var readService = new FakeReadService();
        var handler = new SearchTrainingContractsQueryHandler(readService);
        var query = new SearchTrainingContractsQuery(
            OrganizationId.New(),
            2,
            25,
            "CTR-2026",
            PersonId.New(),
            BranchId.New(),
            TrainingContractStatus.Active,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            null,
            null,
            TrainingContractSortField.StartDate,
            SortDirection.Descending);

        var result = await handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        readService.LastSearch.Should().Be(query);
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(25);
        result.Value.TotalCount.Should().Be(1);
    }

    private sealed class FakeReadService : ITrainingContractReadService
    {
        public SearchTrainingContractsQuery? LastSearch { get; private set; }

        public Task<TrainingContractDetailResponse?> GetAsync(
            OrganizationId organizationId,
            TrainingContractId contractId,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<TrainingContractListItemResponse>> ListAsync(
            OrganizationId organizationId,
            PersonId? studentId,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<PagedResult<TrainingContractListItemResponse>> SearchAsync(
            SearchTrainingContractsQuery query,
            CancellationToken cancellationToken = default)
        {
            LastSearch = query;
            TrainingContractListItemResponse item = new(
                Guid.NewGuid(),
                "CTR-2026-0001",
                Guid.NewGuid(),
                Guid.NewGuid(),
                1,
                nameof(TrainingContractStatus.Active),
                new DateOnly(2026, 8, 1),
                new DateOnly(2027, 8, 1),
                1500m,
                "EUR",
                "B-MANUAL",
                DateTimeOffset.UtcNow);

            return Task.FromResult(new PagedResult<TrainingContractListItemResponse>(
                [item],
                query.PageNumber,
                query.PageSize,
                1));
        }
    }
}
