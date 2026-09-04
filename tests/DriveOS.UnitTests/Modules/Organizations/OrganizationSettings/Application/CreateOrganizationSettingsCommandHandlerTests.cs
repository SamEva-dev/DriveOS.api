using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Application.Branches;
using DriveOS.Modules.Organizations.Application.Branches.Models;
using DriveOS.Modules.Organizations.Application.Branches.StatusHistory;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizationById;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizations;
using DriveOS.Modules.Organizations.Application.Organizations.OrganizationStatusHistory;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.CreateOrganizationSettings;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.OrganizationSettings.Application;

public sealed class CreateOrganizationSettingsCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateAggregateAndCommit_WhenDependenciesAreValid()
    {
        OrganizationId organizationId = OrganizationId.New();
        BranchId branchId = BranchId.New();
        var repository = new FakeOrganizationSettingsRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateOrganizationSettingsCommandHandler(
            new ExistingOrganizationReadService(organizationId),
            new ExistingBranchReadService(organizationId, branchId),
            repository,
            new NoOpReadinessCacheInvalidator(),
            unitOfWork
        );

        CreateOrganizationSettingsCommand command = CreateCommand(
            organizationId,
            branchId,
            requireBranch: true
        );

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(repository.AddedSettings);
        Assert.Equal(organizationId, repository.AddedSettings!.OrganizationId);
        Assert.Equal(branchId, repository.AddedSettings.Operational.DefaultBranchId);
        Assert.Equal(1, unitOfWork.CommitCallCount);
    }

    [Fact]
    public async Task Handle_ShouldFailWithoutCommit_WhenOrganizationDoesNotExist()
    {
        OrganizationId organizationId = OrganizationId.New();
        var repository = new FakeOrganizationSettingsRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateOrganizationSettingsCommandHandler(
            new MissingOrganizationReadService(),
            new MissingBranchReadService(),
            repository,
            new NoOpReadinessCacheInvalidator(),
            unitOfWork
        );

        var result = await handler.Handle(
            CreateCommand(organizationId, null, requireBranch: false),
            CancellationToken.None
        );

        Assert.True(result.IsFailure);
        Assert.Equal(OrganizationErrors.NotFound, result.Error);
        Assert.Null(repository.AddedSettings);
        Assert.Equal(0, unitOfWork.CommitCallCount);
    }

    [Fact]
    public async Task Handle_ShouldFailWithoutCommit_WhenSettingsAlreadyExist()
    {
        OrganizationId organizationId = OrganizationId.New();
        var repository = new FakeOrganizationSettingsRepository { Exists = true };
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateOrganizationSettingsCommandHandler(
            new ExistingOrganizationReadService(organizationId),
            new MissingBranchReadService(),
            repository,
            new NoOpReadinessCacheInvalidator(),
            unitOfWork
        );

        var result = await handler.Handle(
            CreateCommand(organizationId, null, requireBranch: false),
            CancellationToken.None
        );

        Assert.True(result.IsFailure);
        Assert.Equal(OrganizationSettingsErrors.AlreadyExists, result.Error);
        Assert.Null(repository.AddedSettings);
        Assert.Equal(0, unitOfWork.CommitCallCount);
    }

    [Fact]
    public async Task Handle_ShouldRejectBranchOwnedByAnotherOrganization()
    {
        OrganizationId organizationId = OrganizationId.New();
        BranchId branchId = BranchId.New();
        var repository = new FakeOrganizationSettingsRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateOrganizationSettingsCommandHandler(
            new ExistingOrganizationReadService(organizationId),
            new MissingBranchReadService(),
            repository,
            new NoOpReadinessCacheInvalidator(),
            unitOfWork
        );

        var result = await handler.Handle(
            CreateCommand(organizationId, branchId, requireBranch: true),
            CancellationToken.None
        );

        Assert.True(result.IsFailure);
        Assert.Equal(OrganizationSettingsErrors.DefaultBranchNotOwned, result.Error);
        Assert.Null(repository.AddedSettings);
        Assert.Equal(0, unitOfWork.CommitCallCount);
    }

    private static CreateOrganizationSettingsCommand CreateCommand(
        OrganizationId organizationId,
        BranchId? branchId,
        bool requireBranch
    ) =>
        new(
            organizationId,
            "Auto-école Horizon",
            "RCS-123",
            "FR123",
            "contact@horizon.test",
            "+33400000000",
            "https://horizon.test",
            "10 avenue de France",
            null,
            "06000",
            "Nice",
            "Provence-Alpes-Côte d'Azur",
            "FR",
            "fr-FR",
            ["fr-FR", "en-GB"],
            "Europe/Paris",
            "EUR",
            "dd/MM/yyyy",
            "HH:mm",
            DayOfWeek.Monday,
            MeasurementSystem.Metric,
            60,
            120,
            24,
            true,
            requireBranch,
            branchId
        );

    private sealed class ExistingOrganizationReadService(OrganizationId organizationId)
        : IOrganizationReadService
    {
        public Task<OrganizationResponse?> GetByIdAsync(
            OrganizationId requestedId,
            CancellationToken cancellationToken = default
        ) =>
            Task.FromResult<OrganizationResponse?>(
                requestedId == organizationId
                    ? new OrganizationResponse(
                        organizationId.Value,
                        "Auto-école Horizon",
                        "FR",
                        "DrivingSchool",
                        "Active",
                        DateTimeOffset.UtcNow,
                        null,
                        null,
                        null
                    )
                    : null
            );

        public Task<PagedResult<OrganizationListItem>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            OrganizationSortField sortBy,
            SortDirection sortDirection,
            CancellationToken cancellationToken = default
        ) => throw new NotSupportedException();

        public Task<IReadOnlyList<OrganizationStatusHistoryItem>> GetStatusHistoryAsync(
            OrganizationId id,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();
    }

    private sealed class MissingOrganizationReadService : IOrganizationReadService
    {
        public Task<OrganizationResponse?> GetByIdAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<OrganizationResponse?>(null);

        public Task<PagedResult<OrganizationListItem>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            OrganizationSortField sortBy,
            SortDirection sortDirection,
            CancellationToken cancellationToken = default
        ) => throw new NotSupportedException();

        public Task<IReadOnlyList<OrganizationStatusHistoryItem>> GetStatusHistoryAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();
    }

    private sealed class ExistingBranchReadService(OrganizationId organizationId, BranchId branchId)
        : IBranchReadService
    {
        public Task<BranchResponse?> GetByIdAsync(
            OrganizationId requestedOrganizationId,
            BranchId requestedBranchId,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult<BranchResponse?>(
                requestedOrganizationId == organizationId && requestedBranchId == branchId
                    ? new BranchResponse(
                        branchId.Value,
                        organizationId.Value,
                        "Agence principale",
                        "MAIN",
                        "Main",
                        "Active",
                        true,
                        "10 avenue de France",
                        null,
                        "06000",
                        "Nice",
                        "FR",
                        "Europe/Paris",
                        DateTimeOffset.UtcNow,
                        null
                    )
                    : null
            );

        public Task<
            PagedResult<DriveOS.Modules.Organizations.Application.Branches.Models.BranchListItem>
        > GetPagedAsync(
            OrganizationId organizationId,
            int pageNumber,
            int pageSize,
            string? search,
            BranchSortField sortBy,
            SortDirection sortDirection,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();

        public Task<IReadOnlyList<BranchStatusHistoryItem>> GetStatusHistoryAsync(
            OrganizationId organizationId,
            BranchId branchId,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();
    }

    private sealed class MissingBranchReadService : IBranchReadService
    {
        public Task<BranchResponse?> GetByIdAsync(
            OrganizationId organizationId,
            BranchId branchId,
            CancellationToken cancellationToken
        ) => Task.FromResult<BranchResponse?>(null);

        public Task<
            PagedResult<DriveOS.Modules.Organizations.Application.Branches.Models.BranchListItem>
        > GetPagedAsync(
            OrganizationId organizationId,
            int pageNumber,
            int pageSize,
            string? search,
            BranchSortField sortBy,
            SortDirection sortDirection,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();

        public Task<IReadOnlyList<BranchStatusHistoryItem>> GetStatusHistoryAsync(
            OrganizationId organizationId,
            BranchId branchId,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();
    }

    private sealed class NoOpReadinessCacheInvalidator
        : IOrganizationActivationReadinessCacheInvalidator
    {
        public void Invalidate(OrganizationId organizationId) { }
    }
}
