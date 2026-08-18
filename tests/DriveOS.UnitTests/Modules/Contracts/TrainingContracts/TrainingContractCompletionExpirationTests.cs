using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
using Xunit;

namespace DriveOS.UnitTests.Modules.Contracts.TrainingContracts;

public sealed class TrainingContractCompletionExpirationTests
{
    [Fact]
    public void Status_enum_keeps_cancelled_value_and_adds_expired_without_reindexing()
    {
        ((int)TrainingContractStatus.Cancelled).Should().Be(10);
        ((int)TrainingContractStatus.Expired).Should().Be(11);
    }
}
