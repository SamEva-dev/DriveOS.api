using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Application.Leads.QualifyLead;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Leads;

public sealed class QualifyLeadCommandHandlerTests
{
    [Fact]
    public async Task Handle_ForContactedLead_StoresQualificationAndCommits()
    {
        OrganizationId organizationId = OrganizationId.New();
        Lead lead = Lead.Create(LeadId.New(), organizationId, null,
            LeadIdentity.Create("Jane", "Doe", null, "+33600000000").Value,
            RequestedTraining.Create("B", TransmissionPreference.Manual, null).Value,
            LeadSource.Create(LeadSourceType.PhoneCall).Value).Value;
        lead.ChangeStatus(LeadStatus.Contacted);
        var unitOfWork = new FakeCrmUnitOfWork();
        var handler = new QualifyLeadCommandHandler(new FakeLeadRepository(lead), unitOfWork);

        var result = await handler.Handle(new QualifyLeadCommand(organizationId, lead.Id,
            "Permis nécessaire pour un emploi", "B", "Soirs et samedi",
            new DateOnly(2026, 12, 1), FinancingOption.CPF, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        lead.Status.Should().Be(LeadStatus.Qualified);
        lead.Qualification.Should().NotBeNull();
        unitOfWork.CommitCount.Should().Be(1);
    }

    private sealed class FakeLeadRepository(Lead? lead) : ILeadRepository
    {
        public Task<Lead?> GetByIdAsync(OrganizationId organizationId, LeadId id, CancellationToken cancellationToken = default) => Task.FromResult<Lead?>(null);
        public Task<Lead?> GetByIdForUpdateAsync(OrganizationId organizationId, LeadId id, CancellationToken cancellationToken = default) => Task.FromResult(lead);
        public Task<bool> ExistsByEmailAsync(OrganizationId organizationId, string email, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(Lead entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeCrmUnitOfWork : ICrmUnitOfWork
    {
        public int CommitCount { get; private set; }
        public bool HasActiveTransaction => false;
        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<int> CommitAsync(CancellationToken cancellationToken = default) { CommitCount++; return Task.FromResult(1); }
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
