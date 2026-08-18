using DriveOS.Modules.Contracts.Application.TrainingContracts.Activate;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Complete;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Expire;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Suspend;
using DriveOS.Modules.Contracts.Application.TrainingContracts.Terminate;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
using static DriveOS.UnitTests.Modules.Contracts.ContractsApplicationTestFixture;

namespace DriveOS.UnitTests.Modules.Contracts.TrainingContracts;

public sealed class TrainingContractCommandHandlerTests
{
    [Fact]
    public async Task Activate_ShouldTransitionAndCommitOnce()
    {
        DateTimeOffset now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        OrganizationId organizationId = OrganizationId.New();
        TrainingContract contract = CreateSigned(
            organizationId,
            new DateOnly(2026, 8, 1),
            new DateOnly(2027, 8, 1),
            now);
        var uow = new FakeContractsUnitOfWork();
        var handler = new ActivateTrainingContractCommandHandler(
            new FakeTrainingContractRepository(contract),
            uow,
            new FakeClock(now));

        var result = await handler.Handle(
            new ActivateTrainingContractCommand(organizationId, contract.Id, UserId.New()),
            default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(TrainingContractStatus.Active));
        contract.Status.Should().Be(TrainingContractStatus.Active);
        uow.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task Activate_ShouldNotLeakContractAcrossTenants()
    {
        DateTimeOffset now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        TrainingContract contract = CreateSigned(
            OrganizationId.New(),
            new DateOnly(2026, 8, 1),
            null,
            now);
        var uow = new FakeContractsUnitOfWork();
        var handler = new ActivateTrainingContractCommandHandler(
            new FakeTrainingContractRepository(contract),
            uow,
            new FakeClock(now));

        var result = await handler.Handle(
            new ActivateTrainingContractCommand(OrganizationId.New(), contract.Id, UserId.New()),
            default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TrainingContractErrors.NotFound);
        uow.CommitCount.Should().Be(0);
    }

    [Fact]
    public async Task Suspend_ShouldPersistLifecycleAuditAndCommit()
    {
        DateTimeOffset now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        OrganizationId organizationId = OrganizationId.New();
        TrainingContract contract = CreateActive(
            organizationId,
            new DateOnly(2026, 8, 1),
            new DateOnly(2027, 8, 1),
            now);
        UserId actor = UserId.New();
        var uow = new FakeContractsUnitOfWork();
        var handler = new SuspendTrainingContractCommandHandler(
            new FakeTrainingContractRepository(contract),
            uow,
            new FakeClock(now));

        var result = await handler.Handle(
            new SuspendTrainingContractCommand(
                organizationId,
                contract.Id,
                "Temporary administrative suspension",
                new DateOnly(2026, 8, 17),
                new DateOnly(2026, 9, 1),
                actor),
            default);

        result.IsSuccess.Should().BeTrue();
        contract.Status.Should().Be(TrainingContractStatus.Suspended);
        contract.LastModifiedByUserId.Should().Be(actor);
        contract.LastModifiedAtUtc.Should().Be(now);
        uow.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task Terminate_ShouldWorkFromSuspendedAndCommit()
    {
        DateTimeOffset now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        OrganizationId organizationId = OrganizationId.New();
        TrainingContract contract = CreateActive(
            organizationId,
            new DateOnly(2026, 8, 1),
            new DateOnly(2027, 8, 1),
            now);
        contract.Suspend(
            "Temporary administrative suspension",
            new DateOnly(2026, 8, 17),
            null,
            UserId.New(),
            now).IsSuccess.Should().BeTrue();
        var uow = new FakeContractsUnitOfWork();
        var handler = new TerminateTrainingContractCommandHandler(
            new FakeTrainingContractRepository(contract),
            uow,
            new FakeClock(now));

        var result = await handler.Handle(
            new TerminateTrainingContractCommand(
                organizationId,
                contract.Id,
                "Termination requested by the parties",
                new DateOnly(2026, 8, 17),
                UserId.New()),
            default);

        result.IsSuccess.Should().BeTrue();
        contract.Status.Should().Be(TrainingContractStatus.Terminated);
        uow.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task Complete_ShouldCloseActiveContractAndCommit()
    {
        DateTimeOffset now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        OrganizationId organizationId = OrganizationId.New();
        TrainingContract contract = CreateActive(
            organizationId,
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 12, 31),
            now);
        var uow = new FakeContractsUnitOfWork();
        var handler = new CompleteTrainingContractCommandHandler(
            new FakeTrainingContractRepository(contract),
            uow,
            new FakeClock(now));

        var result = await handler.Handle(
            new CompleteTrainingContractCommand(
                organizationId,
                contract.Id,
                "Training contract fully performed",
                new DateOnly(2026, 8, 17),
                UserId.New()),
            default);

        result.IsSuccess.Should().BeTrue();
        contract.Status.Should().Be(TrainingContractStatus.Completed);
        uow.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task Expire_ShouldCloseEndedSignedContractAndCommit()
    {
        DateTimeOffset signedAt = new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        OrganizationId organizationId = OrganizationId.New();
        TrainingContract contract = CreateSigned(
            organizationId,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 8, 16),
            signedAt);
        var uow = new FakeContractsUnitOfWork();
        var handler = new ExpireTrainingContractCommandHandler(
            new FakeTrainingContractRepository(contract),
            uow,
            new FakeClock(now));

        var result = await handler.Handle(
            new ExpireTrainingContractCommand(organizationId, contract.Id, UserId.New()),
            default);

        result.IsSuccess.Should().BeTrue();
        contract.Status.Should().Be(TrainingContractStatus.Expired);
        result.Value.EffectiveDate.Should().Be(new DateOnly(2026, 8, 16));
        uow.CommitCount.Should().Be(1);
    }
}
