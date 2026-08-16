using System.Linq.Expressions;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Application.Organizations.CreateOrganization;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Organizations;

public sealed class CreateOrganizationCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrganizationDoesNotExist_ShouldCreateIt()
    {
        var repository = new FakeOrganizationRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreateOrganizationCommandHandler(repository, unitOfWork);

        var command = new CreateOrganizationCommand(
            "Auto-école Horizon",
            "FR",
            (int)OrganizationType.DrivingSchool
        );

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(repository.AddedOrganization);
        Assert.Equal(1, unitOfWork.CommitCallCount);
    }

    [Fact]
    public async Task Handle_WhenLegalNameAlreadyExists_ShouldFail()
    {
        var repository = new FakeOrganizationRepository { OrganizationExists = true };

        var unitOfWork = new FakeUnitOfWork();

        var handler = new CreateOrganizationCommandHandler(repository, unitOfWork);

        var command = new CreateOrganizationCommand(
            "Auto-école Horizon",
            "FR",
            (int)OrganizationType.DrivingSchool
        );

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(OrganizationErrors.LegalNameAlreadyExists, result.Error);

        Assert.Null(repository.AddedOrganization);
        Assert.Equal(0, unitOfWork.CommitCallCount);
    }

    private sealed class FakeOrganizationRepository : IOrganizationRepository
    {
        public bool OrganizationExists { get; init; }

        public Organization? AddedOrganization { get; private set; }

        public Task<bool> ExistsByLegalNameAsync(
            string legalName,
            string countryCode,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(OrganizationExists);

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

        public Task AddAsync(Organization entity, CancellationToken cancellationToken = default)
        {
            AddedOrganization = entity;
            return Task.CompletedTask;
        }

        public void Update(Organization entity) => throw new NotSupportedException();

        public void Remove(Organization entity) => throw new NotSupportedException();
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int CommitCallCount { get; private set; }

        public bool HasActiveTransaction { get; private set; }

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            HasActiveTransaction = true;
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(1);

        public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            CommitCallCount++;
            return Task.FromResult(1);
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            HasActiveTransaction = false;
            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            HasActiveTransaction = false;
            return Task.CompletedTask;
        }
    }
}
