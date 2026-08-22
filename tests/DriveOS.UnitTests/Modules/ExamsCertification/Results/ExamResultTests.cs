using DriveOS.Modules.ExamsCertification.Domain.Results;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Modules.ExamsCertification.Results;

public sealed class ExamResultTests
{
    private static ExamResult Create(ExamResultOutcome outcome = ExamResultOutcome.Failed)
    {
        var result = ExamResult.Create(new OrganizationId(Guid.NewGuid()), new ExamAttemptId(Guid.NewGuid()),
            new ExamRegistrationId(Guid.NewGuid()), new PersonId(Guid.NewGuid()), 1, outcome, 24m, "Observation",
            "Initial", ExamResultSourceKind.Manual, "driveos-manual", null, null, DateTimeOffset.UtcNow,
            Guid.NewGuid(), "fingerprint-1", new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow);
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    [Fact]
    public void Create_ShouldCreateFirstImmutableRevision()
    {
        ExamResult result = Create();
        Assert.Equal(1, result.CurrentRevision);
        Assert.Single(result.Revisions);
        Assert.Equal(ExamResultStatus.Recorded, result.Status);
    }

    [Fact]
    public void Finalize_RequiresVerification()
    {
        ExamResult result = Create();
        Assert.True(result.Finalize(new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow).IsFailure);
    }

    [Fact]
    public void VerifyThenFinalize_ShouldFinalize()
    {
        ExamResult result = Create();
        Assert.True(result.Verify("manual-check", new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow).IsSuccess);
        Assert.True(result.Finalize(new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow).IsSuccess);
        Assert.Equal(ExamResultStatus.Finalized, result.Status);
    }

    [Fact]
    public void Correction_ShouldAppendRevisionAndRequireReverification()
    {
        ExamResult result = Create();
        var actor = new UserId(Guid.NewGuid());
        Assert.True(result.Verify("verified", actor, DateTimeOffset.UtcNow).IsSuccess);
        Assert.True(result.Finalize(actor, DateTimeOffset.UtcNow).IsSuccess);
        Assert.True(result.Correct(ExamResultOutcome.Passed, 31m, null, "Official correction", ExamResultSourceKind.OfficialApi,
            "official-provider", "external-2", null, DateTimeOffset.UtcNow, "Authority corrected the result", Guid.NewGuid(),
            "fingerprint-2", actor, DateTimeOffset.UtcNow).IsSuccess);
        Assert.Equal(2, result.CurrentRevision);
        Assert.Equal(2, result.Revisions.Count);
        Assert.Equal(ExamResultOutcome.Passed, result.Outcome);
        Assert.Equal(ExamResultStatus.Recorded, result.Status);
        Assert.Null(result.FinalizedAtUtc);
    }

    [Fact]
    public void Correction_WithSameOperationAndDifferentPayload_ShouldConflict()
    {
        ExamResult result = Create();
        var actor = new UserId(Guid.NewGuid());
        Guid op = Guid.NewGuid();
        Assert.True(result.Correct(ExamResultOutcome.Failed, 20m, "A", null, ExamResultSourceKind.Manual, "driveos-manual", null,
            null, DateTimeOffset.UtcNow, "Correction", op, "same", actor, DateTimeOffset.UtcNow).IsSuccess);
        Assert.True(result.Correct(ExamResultOutcome.Passed, 30m, null, null, ExamResultSourceKind.Manual, "driveos-manual", null,
            null, DateTimeOffset.UtcNow, "Other", op, "different", actor, DateTimeOffset.UtcNow).IsFailure);
    }
}
