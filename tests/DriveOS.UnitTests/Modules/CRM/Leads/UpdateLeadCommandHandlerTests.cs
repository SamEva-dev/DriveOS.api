using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Application.Leads.UpdateLead;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Leads;

public sealed class UpdateLeadCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingLead_UpdatesInformationAndCommits()
    {
        OrganizationId organizationId = OrganizationId.New();
        Lead lead = CreateLead(organizationId);
        var repository = new FakeLeadRepository(lead);
        var unitOfWork = new FakeCrmUnitOfWork();
        var handler = new UpdateLeadCommandHandler(repository, unitOfWork);

        Result result = await handler.Handle(
            CreateCommand(organizationId, lead.Id),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        repository.RequestedOrganizationId.Should().Be(organizationId);
        repository.RequestedLeadId.Should().Be(lead.Id);
        lead.Identity.FirstName.Should().Be("John");
        lead.Identity.Email.Should().Be("john.smith@example.com");
        lead.RequestedTraining.LicenseCategory.Should().Be("A2");
        lead.Source.Type.Should().Be(LeadSourceType.Referral);
        lead.Status.Should().Be(LeadStatus.New);
        lead.AssignedAdvisorId.Should().NotBeNull();
        unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithLeadOutsideTenant_ReturnsNotFoundWithoutCommit()
    {
        var repository = new FakeLeadRepository(null);
        var unitOfWork = new FakeCrmUnitOfWork();
        var handler = new UpdateLeadCommandHandler(repository, unitOfWork);

        Result result = await handler.Handle(
            CreateCommand(OrganizationId.New(), LeadId.New()),
            CancellationToken.None
        );

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Crm.Leads.NotFound");
        unitOfWork.CommitCount.Should().Be(0);
    }

    private static UpdateLeadCommand CreateCommand(OrganizationId organizationId, LeadId leadId) =>
        new(
            organizationId,
            leadId,
            BranchId.New(),
            " John ",
            " Smith ",
            " JOHN.SMITH@EXAMPLE.COM ",
            "+33611111111",
            " a2 ",
            TransmissionPreference.Manual,
            "Nice Ouest",
            LeadSourceType.Referral,
            "Ancien élève"
        );

    private static Lead CreateLead(OrganizationId organizationId)
    {
        LeadIdentity identity = LeadIdentity.Create("Jane", "Doe", "jane@example.com", null).Value;
        RequestedTraining training = RequestedTraining
            .Create("B", TransmissionPreference.Automatic, null)
            .Value;
        LeadSource source = LeadSource.Create(LeadSourceType.Website).Value;

        return Lead.Create(
            LeadId.New(),
            organizationId,
            BranchId.New(),
            identity,
            training,
            source,
            UserId.New()
        ).Value;
    }

    private sealed class FakeLeadRepository(Lead? lead) : ILeadRepository
    {
        public OrganizationId? RequestedOrganizationId { get; private set; }
        public LeadId? RequestedLeadId { get; private set; }

        public Task<Lead?> GetByIdAsync(
            OrganizationId organizationId,
            LeadId id,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<Lead?>(null);

        public Task<Lead?> GetByIdForUpdateAsync(
            OrganizationId organizationId,
            LeadId id,
            CancellationToken cancellationToken = default
        )
        {
            RequestedOrganizationId = organizationId;
            RequestedLeadId = id;
            return Task.FromResult(lead);
        }

        public Task<bool> ExistsByEmailAsync(
            OrganizationId organizationId,
            string email,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(false);

        public Task AddAsync(Lead entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
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
