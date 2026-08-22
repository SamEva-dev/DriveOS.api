using DriveOS.Modules.ExamsCertification.Domain.Registrations.Convocations;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.ExamsCertification.Registrations.Convocations;

public sealed class ExamConvocationTests
{
    [Fact]
    public void ReceiveOfficialRevision_ShouldAppendVersions_AndResetDelivery()
    {
        UserId actor = new(Guid.NewGuid());
        ExamConvocation convocation = ExamConvocation.Create(
            ExamConvocationId.New(), new OrganizationId(Guid.NewGuid()), new ExamRegistrationId(Guid.NewGuid()),
            new PersonId(Guid.NewGuid()), actor, DateTimeOffset.UtcNow).Value;

        DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(10);
        var first = convocation.ReceiveOfficialRevision(
            ExamConvocationRevisionId.New(), new ExamCenterId(Guid.NewGuid()), "Nice", "1 rue test", "Europe/Paris",
            start, start.AddMinutes(32), "manual", "OFF-1", "NEPH", "Arriver 30 min avant", "Pièce d'identité",
            null, Guid.NewGuid(), "FP1", actor, DateTimeOffset.UtcNow);
        first.IsSuccess.Should().BeTrue();
        convocation.MarkDelivered(ExamConvocationDeliveryChannel.Portal, actor, DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();

        var second = convocation.ReceiveOfficialRevision(
            ExamConvocationRevisionId.New(), new ExamCenterId(Guid.NewGuid()), "Cannes", null, "Europe/Paris",
            start.AddDays(1), start.AddDays(1).AddMinutes(32), "manual", "OFF-2", "NEPH", null, null,
            null, Guid.NewGuid(), "FP2", actor, DateTimeOffset.UtcNow);

        second.IsSuccess.Should().BeTrue();
        convocation.CurrentVersion.Should().Be(2);
        convocation.Revisions.Should().HaveCount(2);
        convocation.DeliveryStatus.Should().Be(ExamConvocationDeliveryStatus.Pending);
        convocation.DeliveredAtUtc.Should().BeNull();
    }

    [Fact]
    public void ReceiveOfficialRevision_ShouldBeIdempotentByOperationId()
    {
        UserId actor = new(Guid.NewGuid());
        ExamConvocation convocation = ExamConvocation.Create(
            ExamConvocationId.New(), new OrganizationId(Guid.NewGuid()), new ExamRegistrationId(Guid.NewGuid()),
            new PersonId(Guid.NewGuid()), actor, DateTimeOffset.UtcNow).Value;
        Guid operationId = Guid.NewGuid();
        ExamCenterId centerId = new(Guid.NewGuid());
        DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(10);

        convocation.ReceiveOfficialRevision(ExamConvocationRevisionId.New(), centerId, "Nice", null, "Europe/Paris",
            start, start.AddMinutes(32), "manual", null, null, null, null, null, operationId, "FP", actor, DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();

        var replay = convocation.ReceiveOfficialRevision(ExamConvocationRevisionId.New(), centerId, "Nice", null, "Europe/Paris",
            start, start.AddMinutes(32), "manual", null, null, null, null, null, operationId, "FP", actor, DateTimeOffset.UtcNow);
        var conflict = convocation.ReceiveOfficialRevision(ExamConvocationRevisionId.New(), centerId, "Nice", null, "Europe/Paris",
            start.AddHours(1), start.AddHours(1).AddMinutes(32), "manual", null, null, null, null, null, operationId, "OTHER", actor, DateTimeOffset.UtcNow);

        replay.IsSuccess.Should().BeTrue();
        convocation.Revisions.Should().HaveCount(1);
        conflict.IsFailure.Should().BeTrue();
        conflict.Error.Code.Should().Be("Exams.Convocation.OperationConflict");
    }
}
