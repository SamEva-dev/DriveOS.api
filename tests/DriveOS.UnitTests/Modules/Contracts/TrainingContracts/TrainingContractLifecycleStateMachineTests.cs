using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
using static DriveOS.UnitTests.Modules.Contracts.ContractsApplicationTestFixture;

namespace DriveOS.UnitTests.Modules.Contracts.TrainingContracts;

public sealed class TrainingContractLifecycleStateMachineTests
{
    [Fact]
    public void CompletedContract_ShouldBeTerminalForOperationalTransitions()
    {
        DateTimeOffset now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        TrainingContract contract = CreateActive(
            OrganizationId.New(),
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 12, 31),
            now);
        contract.Complete(
            "Training contract fully performed",
            new DateOnly(2026, 8, 17),
            UserId.New(),
            now).IsSuccess.Should().BeTrue();

        contract.Suspend(
            "Suspension after completion is invalid",
            new DateOnly(2026, 8, 17),
            null,
            UserId.New(),
            now).Error.Should().Be(TrainingContractErrors.SuspensionNotAllowed);
        contract.Terminate(
            "Termination after completion is invalid",
            new DateOnly(2026, 8, 17),
            UserId.New(),
            now).Error.Should().Be(TrainingContractErrors.TerminationNotAllowed);
        contract.Expire(UserId.New(), now.AddYears(1)).Error
            .Should().Be(TrainingContractErrors.ExpirationNotAllowed);
    }

    [Fact]
    public void TerminatedContract_ShouldBeTerminalForOperationalTransitions()
    {
        DateTimeOffset now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        TrainingContract contract = CreateActive(
            OrganizationId.New(),
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 12, 31),
            now);
        contract.Terminate(
            "Termination requested by the parties",
            new DateOnly(2026, 8, 17),
            UserId.New(),
            now).IsSuccess.Should().BeTrue();

        contract.Suspend(
            "Suspension after termination is invalid",
            new DateOnly(2026, 8, 17),
            null,
            UserId.New(),
            now).Error.Should().Be(TrainingContractErrors.SuspensionNotAllowed);
        contract.Complete(
            "Completion after termination is invalid",
            new DateOnly(2026, 8, 17),
            UserId.New(),
            now).Error.Should().Be(TrainingContractErrors.CompletionNotAllowed);
    }

    [Fact]
    public void ExpiredContract_ShouldBeTerminalForOperationalTransitions()
    {
        DateTimeOffset signedAt = new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset now = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
        TrainingContract contract = CreateSigned(
            OrganizationId.New(),
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 8, 16),
            signedAt);
        contract.Expire(UserId.New(), now).IsSuccess.Should().BeTrue();

        contract.Activate(UserId.New(), now).Error
            .Should().Be(TrainingContractErrors.ActivationNotAllowed);
        contract.Terminate(
            "Termination after expiration is invalid",
            new DateOnly(2026, 8, 17),
            UserId.New(),
            now).Error.Should().Be(TrainingContractErrors.TerminationNotAllowed);
    }
}
