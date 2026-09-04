using System.Linq.Expressions;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.Organizations.ProvisionOrganization;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Organizations;

public sealed class ProvisionOrganizationCommandHandlerTests
{
    [Fact]
    public async Task Handle_FirstRequest_ShouldCreateOrganizationAndPrimaryOwnerAtomically()
    {
        var organizations = new FakeOrganizationRepository();
        var representatives = new FakeRepresentativeRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new ProvisionOrganizationCommandHandler(
            organizations,
            representatives,
            unitOfWork,
            TimeProvider.System
        );
        var externalUserId = new UserId(Guid.NewGuid());

        var result = await handler.Handle(
            new ProvisionOrganizationCommand(
                externalUserId,
                "authgate-org-stable",
                "Auto-école Horizon",
                "FR",
                (int)OrganizationType.DrivingSchool
            ),
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.WasCreated);
        Assert.NotNull(organizations.Added);
        Assert.NotNull(representatives.Added);
        Assert.True(representatives.Added.IsPrimaryOwner);
        Assert.Equal(OrganizationRepresentativeStatus.Active, representatives.Added.Status);
        Assert.Equal(externalUserId, representatives.Added.UserId);
        Assert.Equal(1, unitOfWork.CommitCount);
    }

    [Fact]
    public async Task Handle_RetryWithSamePayload_ShouldReturnExistingOrganization()
    {
        var externalUserId = new UserId(Guid.NewGuid());
        Organization existing = Organization.Create(
            OrganizationId.New(),
            "Auto-école Horizon",
            "FR",
            OrganizationType.DrivingSchool
        ).Value;
        existing.SetProvisioningIdentity(externalUserId, "authgate-org-stable");
        var organizations = new FakeOrganizationRepository { Existing = existing };
        var representatives = new FakeRepresentativeRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new ProvisionOrganizationCommandHandler(
            organizations,
            representatives,
            unitOfWork,
            TimeProvider.System
        );

        var result = await handler.Handle(
            new ProvisionOrganizationCommand(
                externalUserId,
                "authgate-org-stable",
                "Auto-école Horizon",
                "FR",
                (int)OrganizationType.DrivingSchool
            ),
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.WasCreated);
        Assert.Equal(existing.Id, result.Value.OrganizationId);
        Assert.Null(representatives.Added);
        Assert.Equal(0, unitOfWork.CommitCount);
    }

    private sealed class FakeRepresentativeRepository : IOrganizationRepresentativeRepository
    {
        public OrganizationRepresentative? Added { get; private set; }
        public Task<OrganizationRepresentative?> GetForUpdateAsync(
            OrganizationRepresentativeId representativeId,
            OrganizationId organizationId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<OrganizationRepresentative?>(null);
        public Task<bool> ExistsActiveAsync(
            OrganizationId organizationId,
            PersonId personId,
            OrganizationRepresentativeType representativeType,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(false);
        public Task<int> CountActiveOwnersAsync(
            OrganizationId organizationId,
            OrganizationRepresentativeId? excludingRepresentativeId = null,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(0);
        public Task<OrganizationRepresentative?> GetPrimaryOwnerForUpdateAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<OrganizationRepresentative?>(null);
        public Task AddAsync(
            OrganizationRepresentative representative,
            CancellationToken cancellationToken = default
        )
        {
            Added = representative;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeOrganizationRepository : IOrganizationRepository
    {
        public Organization? Existing { get; init; }
        public Organization? Added { get; private set; }
        public Task<Organization?> GetByProvisioningKeyAsync(
            string idempotencyKey,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(Existing);
        public Task<bool> ExistsByLegalNameAsync(
            string legalName,
            string countryCode,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(false);
        public Task<Organization?> GetByIdAsync(
            OrganizationId id,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<Organization?>(null);
        public Task<IReadOnlyCollection<Organization>> GetAllAsync(
            bool asNoTracking = false,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<IReadOnlyCollection<Organization>>([]);
        public Task<IReadOnlyCollection<Organization>> FindAsync(
            Expression<Func<Organization, bool>> predicate,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<IReadOnlyCollection<Organization>>([]);
        public Task<Organization?> FirstOrDefaultAsync(
            Expression<Func<Organization, bool>> predicate,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<Organization?>(null);
        public Task<int> CountAsync(
            Expression<Func<Organization, bool>>? predicate = null,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(0);
        public Task AddAsync(
            Organization entity,
            CancellationToken cancellationToken = default
        )
        {
            Added = entity;
            return Task.CompletedTask;
        }
        public void Update(Organization entity) => throw new NotSupportedException();
        public void Remove(Organization entity) => throw new NotSupportedException();
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int CommitCount { get; private set; }
        public bool HasActiveTransaction => false;
        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
        public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            CommitCount++;
            return Task.FromResult(2);
        }
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
