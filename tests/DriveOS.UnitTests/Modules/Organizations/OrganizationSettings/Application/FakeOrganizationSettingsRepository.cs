using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.OrganizationSettings.Application;

internal sealed class FakeOrganizationSettingsRepository : IOrganizationSettingsRepository
{
    public global::DriveOS.Modules.Organizations.Domain.OrganizationSettings.OrganizationSettings? Settings { get; set; }
    public global::DriveOS.Modules.Organizations.Domain.OrganizationSettings.OrganizationSettings? AddedSettings { get; private set; }
    public bool Exists { get; set; }

    public Task<global::DriveOS.Modules.Organizations.Domain.OrganizationSettings.OrganizationSettings?> GetForUpdateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default) => Task.FromResult(Settings);

    public Task<bool> ExistsAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default) => Task.FromResult(Exists);

    public Task AddAsync(
        global::DriveOS.Modules.Organizations.Domain.OrganizationSettings.OrganizationSettings settings,
        CancellationToken cancellationToken = default)
    {
        AddedSettings = settings;
        return Task.CompletedTask;
    }
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int CommitCallCount { get; private set; }
    public bool HasActiveTransaction { get; private set; }
    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) { HasActiveTransaction = true; return Task.CompletedTask; }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    public Task<int> CommitAsync(CancellationToken cancellationToken = default) { CommitCallCount++; return Task.FromResult(1); }
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default) { HasActiveTransaction = false; return Task.CompletedTask; }
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) { HasActiveTransaction = false; return Task.CompletedTask; }
}
