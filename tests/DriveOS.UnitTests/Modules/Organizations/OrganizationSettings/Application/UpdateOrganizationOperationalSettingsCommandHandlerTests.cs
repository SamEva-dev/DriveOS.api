using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application.Branches;
using DriveOS.Modules.Organizations.Application.Branches.Models;
using DriveOS.Modules.Organizations.Application.Branches.StatusHistory;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateOperationalSettings;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.OrganizationSettings.Application;

public sealed class UpdateOrganizationOperationalSettingsCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRejectUnknownDefaultBranchWithoutMutatingAggregate()
    {
        var settings = OrganizationSettingsTestData.CreateAggregate();
        var repository = new FakeOrganizationSettingsRepository { Settings = settings };
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UpdateOrganizationOperationalSettingsCommandHandler(
            repository,
            new MissingBranchReadService(),
            unitOfWork);
        int initialVersion = settings.Version;

        var result = await handler.Handle(
            new UpdateOrganizationOperationalSettingsCommand(
                settings.OrganizationId,
                90,
                60,
                48,
                false,
                true,
                BranchId.New(),
                initialVersion),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(OrganizationSettingsErrors.DefaultBranchNotOwned, result.Error);
        Assert.Equal(initialVersion, settings.Version);
        Assert.Equal(0, unitOfWork.CommitCallCount);
    }

    [Fact]
    public async Task Handle_ShouldFailWithoutReadingBranch_WhenVersionIsStale()
    {
        var settings = OrganizationSettingsTestData.CreateAggregate();
        var repository = new FakeOrganizationSettingsRepository { Settings = settings };
        var unitOfWork = new FakeUnitOfWork();
        var branchReadService = new CountingBranchReadService();
        var handler = new UpdateOrganizationOperationalSettingsCommandHandler(
            repository,
            branchReadService,
            unitOfWork);

        var result = await handler.Handle(
            new UpdateOrganizationOperationalSettingsCommand(
                settings.OrganizationId,
                90,
                60,
                48,
                false,
                true,
                BranchId.New(),
                settings.Version + 1),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(OrganizationSettingsErrors.ConcurrentUpdate, result.Error);
        Assert.Equal(0, branchReadService.GetByIdCallCount);
        Assert.Equal(0, unitOfWork.CommitCallCount);
    }

    private sealed class MissingBranchReadService : IBranchReadService
    {
        public Task<BranchResponse?> GetByIdAsync(
            OrganizationId organizationId,
            BranchId branchId,
            CancellationToken cancellationToken) =>
            Task.FromResult<BranchResponse?>(null);

        public Task<PagedResult<BranchListItem>> GetPagedAsync(
            OrganizationId organizationId,
            int pageNumber,
            int pageSize,
            string? search,
            BranchSortField sortBy,
            SortDirection sortDirection,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<BranchStatusHistoryItem>> GetStatusHistoryAsync(
            OrganizationId organizationId,
            BranchId branchId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class CountingBranchReadService : IBranchReadService
    {
        public int GetByIdCallCount { get; private set; }

        public Task<BranchResponse?> GetByIdAsync(
            OrganizationId organizationId,
            BranchId branchId,
            CancellationToken cancellationToken)
        {
            GetByIdCallCount++;
            return Task.FromResult<BranchResponse?>(null);
        }

        public Task<PagedResult<BranchListItem>> GetPagedAsync(
            OrganizationId organizationId,
            int pageNumber,
            int pageSize,
            string? search,
            BranchSortField sortBy,
            SortDirection sortDirection,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<BranchStatusHistoryItem>> GetStatusHistoryAsync(
            OrganizationId organizationId,
            BranchId branchId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
