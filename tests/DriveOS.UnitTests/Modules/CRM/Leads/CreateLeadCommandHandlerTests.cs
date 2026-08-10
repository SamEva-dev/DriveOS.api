using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Application.Leads.CreateLead;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Leads;

public sealed class CreateLeadCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidData_PersistsLeadAndCommits()
    {
        var repository = new FakeLeadRepository();
        var unitOfWork = new FakeCrmUnitOfWork();
        var handler = new CreateLeadCommandHandler(repository, unitOfWork);
        OrganizationId organizationId = OrganizationId.New();

        var result = await handler.Handle(
            CreateCommand(organizationId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repository.AddedLead.Should().NotBeNull();
        repository.AddedLead!.OrganizationId.Should().Be(organizationId);
        repository.AddedLead.Identity.Email.Should().Be("jane.doe@example.com");
        repository.AddedLead.Status.Should().Be(LeadStatus.New);
        unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithInvalidIdentity_DoesNotPersistOrCommit()
    {
        var repository = new FakeLeadRepository();
        var unitOfWork = new FakeCrmUnitOfWork();
        var handler = new CreateLeadCommandHandler(repository, unitOfWork);
        CreateLeadCommand command = CreateCommand(OrganizationId.New()) with
        {
            FirstName = " "
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Crm.Leads.FirstName.Required");
        repository.AddedLead.Should().BeNull();
        unitOfWork.CommitCount.Should().Be(0);
    }

    private static CreateLeadCommand CreateCommand(OrganizationId organizationId) =>
        new(
            organizationId,
            BranchId.New(),
            " Jane ",
            " Doe ",
            " JANE.DOE@EXAMPLE.COM ",
            "+33600000000",
            " b ",
            TransmissionPreference.Manual,
            "Nice Centre",
            LeadSourceType.Website,
            null,
            UserId.New());

    private sealed class FakeLeadRepository : ILeadRepository
    {
        public Lead? AddedLead { get; private set; }

        public Task<Lead?> GetByIdAsync(
            OrganizationId organizationId,
            LeadId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Lead?>(null);

        public Task<Lead?> GetByIdForUpdateAsync(
            OrganizationId organizationId,
            LeadId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Lead?>(null);

        public Task<bool> ExistsByEmailAsync(
            OrganizationId organizationId,
            string email,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task AddAsync(
            Lead lead,
            CancellationToken cancellationToken = default)
        {
            AddedLead = lead;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCrmUnitOfWork : ICrmUnitOfWork
    {
        public int CommitCount { get; private set; }
        public bool HasActiveTransaction => false;

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            CommitCount++;
            return Task.FromResult(1);
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
