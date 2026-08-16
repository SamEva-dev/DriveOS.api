using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Application.Activities.ImportActivity;
using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Modules.CRM.Activities;

public sealed class ImportCrmActivityCommandHandlerTests
{
    [Fact]
    public async Task ExistingKey_ShouldReturnExistingActivityAfterAcquiringTheLock()
    {
        OrganizationId organizationId = new(Guid.NewGuid());
        CrmActivity existing = CrmActivity
            .Create(
                CrmActivityId.New(),
                organizationId,
                null,
                CrmActivityType.Email,
                CrmActivityDirection.Inbound,
                "Import",
                null,
                DateTimeOffset.UtcNow,
                null,
                CrmActivityMetadata.Imported(
                    "ext",
                    "key",
                    CrmActivitySyncStatus.Synchronized,
                    DateTimeOffset.UtcNow
                )
            )
            .Value;
        var repository = new FakeActivityRepository(existing);
        var unitOfWork = new FakeUnitOfWork();
        var importLock = new FakeImportLock(unitOfWork);
        var handler = new ImportCrmActivityCommandHandler(
            new FakeLeadRepository(),
            repository,
            importLock,
            unitOfWork,
            new FakeClock(DateTimeOffset.UtcNow)
        );

        var result = await handler.Handle(Command(organizationId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.AlreadyImported);
        Assert.Equal(existing.Id.Value, result.Value.ActivityId);
        Assert.True(importLock.AcquiredInsideTransaction);
        Assert.False(repository.AddCalled);
        Assert.True(unitOfWork.RollbackCalled);
    }

    private static ImportCrmActivityCommand Command(OrganizationId organizationId) =>
        new(
            organizationId,
            null,
            CrmActivityType.Email,
            CrmActivityDirection.Inbound,
            "Import",
            null,
            DateTimeOffset.UtcNow,
            null,
            "ext",
            "key",
            CrmActivitySyncStatus.Synchronized,
            null,
            null,
            null,
            false,
            null,
            null
        );

    private sealed class FakeImportLock(FakeUnitOfWork unitOfWork) : ICrmActivityImportLock
    {
        public bool AcquiredInsideTransaction { get; private set; }

        public Task AcquireAsync(
            OrganizationId organizationId,
            string idempotencyKey,
            CancellationToken ct
        )
        {
            AcquiredInsideTransaction = unitOfWork.HasActiveTransaction;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeActivityRepository(CrmActivity existing) : ICrmActivityRepository
    {
        public bool AddCalled { get; private set; }

        public void Add(CrmActivity activity) => AddCalled = true;

        public Task<CrmActivity?> GetByIdempotencyKeyAsync(
            OrganizationId organizationId,
            string idempotencyKey,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<CrmActivity?>(existing);

        public Task<IReadOnlyList<CrmActivity>> GetByLeadAsync(
            OrganizationId organizationId,
            LeadId leadId,
            CancellationToken cancellationToken = default
        ) => throw new NotSupportedException();

        public Task<IReadOnlyList<CrmActivity>> GetRecentAsync(
            OrganizationId organizationId,
            int limit,
            CancellationToken cancellationToken = default
        ) => throw new NotSupportedException();
    }

    private sealed class FakeUnitOfWork : ICrmUnitOfWork
    {
        public bool HasActiveTransaction { get; private set; }
        public bool RollbackCalled { get; private set; }

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            HasActiveTransaction = true;
            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            HasActiveTransaction = false;
            RollbackCalled = true;
            return Task.CompletedTask;
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            HasActiveTransaction = false;
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task<int> CommitAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
    }

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow => utcNow;
    }

    private sealed class FakeLeadRepository : ILeadRepository
    {
        public Task<Lead?> GetByIdAsync(
            OrganizationId organizationId,
            LeadId id,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<Lead?>(null);

        public Task<Lead?> GetByIdForUpdateAsync(
            OrganizationId organizationId,
            LeadId id,
            CancellationToken cancellationToken = default
        ) => throw new NotSupportedException();

        public Task<bool> ExistsByEmailAsync(
            OrganizationId organizationId,
            string email,
            CancellationToken cancellationToken = default
        ) => throw new NotSupportedException();

        public Task AddAsync(Lead lead, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
