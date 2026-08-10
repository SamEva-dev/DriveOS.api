using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Application.Leads.ChangeLeadStatus;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Leads;

public sealed class ChangeLeadStatusCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithAllowedTransition_ChangesStatusAndCommits()
    {
        OrganizationId organizationId = OrganizationId.New();
        Lead lead = CreateLead(organizationId);
        var repository = new FakeLeadRepository(lead);
        var unitOfWork = new FakeCrmUnitOfWork();
        var handler = new ChangeLeadStatusCommandHandler(repository, unitOfWork);

        Result result = await handler.Handle(
            new ChangeLeadStatusCommand(
                organizationId,
                lead.Id,
                LeadStatus.Contacted,
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        lead.Status.Should().Be(LeadStatus.Contacted);
        repository.RequestedOrganizationId.Should().Be(organizationId);
        unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithLeadOutsideTenant_ReturnsNotFoundWithoutCommit()
    {
        var unitOfWork = new FakeCrmUnitOfWork();
        var handler = new ChangeLeadStatusCommandHandler(
            new FakeLeadRepository(null),
            unitOfWork);

        Result result = await handler.Handle(
            new ChangeLeadStatusCommand(
                OrganizationId.New(),
                LeadId.New(),
                LeadStatus.Contacted,
                null),
            CancellationToken.None);

        result.Error.Code.Should().Be("Crm.Leads.NotFound");
        unitOfWork.CommitCount.Should().Be(0);
    }

    private static Lead CreateLead(OrganizationId organizationId) =>
        Lead.Create(
            LeadId.New(),
            organizationId,
            null,
            LeadIdentity.Create("Jane", "Doe", null, "+33600000000").Value,
            RequestedTraining.Create("B", TransmissionPreference.Manual, null).Value,
            LeadSource.Create(LeadSourceType.PhoneCall).Value).Value;

    private sealed class FakeLeadRepository(Lead? lead) : ILeadRepository
    {
        public OrganizationId? RequestedOrganizationId { get; private set; }

        public Task<Lead?> GetByIdAsync(OrganizationId organizationId, LeadId id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Lead?>(null);

        public Task<Lead?> GetByIdForUpdateAsync(OrganizationId organizationId, LeadId id, CancellationToken cancellationToken = default)
        {
            RequestedOrganizationId = organizationId;
            return Task.FromResult(lead);
        }

        public Task<bool> ExistsByEmailAsync(OrganizationId organizationId, string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task AddAsync(Lead entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeCrmUnitOfWork : ICrmUnitOfWork
    {
        public int CommitCount { get; private set; }
        public bool HasActiveTransaction => false;
        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            CommitCount++;
            return Task.FromResult(1);
        }
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
