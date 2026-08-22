using DriveOS.Modules.ExamsCertification.Domain.Registrations;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.ExamsCertification.Registrations;

public sealed class ExamRegistrationTests
{
    [Fact]
    public void Create_ShouldFreezePlaceAndReadinessSnapshotReferences()
    {
        ExamRegistrationId id = ExamRegistrationId.New();
        ExamReadinessDecisionId readinessId = ExamReadinessDecisionId.New();
        ExamPlaceId placeId = ExamPlaceId.New();
        ExamCenterId centerId = ExamCenterId.New();
        DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(3);

        var result = ExamRegistration.Create(
            id, new OrganizationId(Guid.NewGuid()), new PersonId(Guid.NewGuid()), new TrainingPathId(Guid.NewGuid()),
            readinessId, placeId, centerId, "Practical", "B", start, start.AddMinutes(32), "manual", null,
            Guid.NewGuid(), "ABC", new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.ReadinessDecisionId.Should().Be(readinessId);
        result.Value.ExamPlaceId.Should().Be(placeId);
        result.Value.ExamCenterId.Should().Be(centerId);
        result.Value.Status.Should().Be(ExamRegistrationStatus.PlaceAssigned);
    }

    [Fact]
    public void MatchesOperation_ShouldRejectSameOperationWithDifferentFingerprint()
    {
        Guid operationId = Guid.NewGuid();
        DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(3);
        var result = ExamRegistration.Create(
            ExamRegistrationId.New(), new OrganizationId(Guid.NewGuid()), new PersonId(Guid.NewGuid()), new TrainingPathId(Guid.NewGuid()),
            ExamReadinessDecisionId.New(), ExamPlaceId.New(), ExamCenterId.New(), "Practical", "B", start, start.AddMinutes(32), "manual", null,
            operationId, "A", new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow);

        result.Value.MatchesOperation(operationId, "A").Should().BeTrue();
        result.Value.MatchesOperation(operationId, "B").Should().BeFalse();
    }
}
