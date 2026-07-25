using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.Organizations.CreateOrganization;
using DriveOS.Modules.Organizations.Application.Abstractions;

namespace DriveOS.UnitTests.Organizations;

public sealed class CreateOrganizationCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrganizationDoesNotExist_ShouldCreateIt()
    {
        var repository = new FakeOrganizationRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreateOrganizationCommandHandler(
            repository,
            unitOfWork);

        var command = new CreateOrganizationCommand(
            "Auto-école Horizon",
            "FR",
            (int)OrganizationType.DrivingSchool);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(repository.AddedOrganization);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenLegalNameAlreadyExists_ShouldFail()
    {
        var repository = new FakeOrganizationRepository
        {
            OrganizationExists = true
        };

        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreateOrganizationCommandHandler(
            repository,
            unitOfWork);

        var command = new CreateOrganizationCommand(
            "Auto-école Horizon",
            "FR",
            (int)OrganizationType.DrivingSchool);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            OrganizationErrors.LegalNameAlreadyExists,
            result.Error);

        Assert.Null(repository.AddedOrganization);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private sealed class FakeOrganizationRepository
        : IOrganizationRepository
    {
        public bool OrganizationExists { get; init; }

        public Organization? AddedOrganization { get; private set; }

        public Task<bool> ExistsByLegalNameAsync(
            string legalName,
            string countryCode,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(OrganizationExists);
        }

        public void Add(Organization organization)
        {
            AddedOrganization = organization;
        }

        public Task<Organization?> GetByIdAsync(
            OrganizationId organizationId,
            QueryTracking tracking = QueryTracking.NoTracking,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Organization?>(null);
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;

            return Task.FromResult(1);
        }
    }
}