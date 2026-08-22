using DriveOS.Modules.ExamsCertification.Domain.Results.Failure;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Modules.ExamsCertification.Results;

public sealed class ExamFailureAnalysisTests
{
    private static ExamFailureAnalysis Create(string? officialReason = "OBSERVATION") => ExamFailureAnalysis.Create(
        new OrganizationId(Guid.NewGuid()), new ExamResultId(Guid.NewGuid()), 1, new ExamAttemptId(Guid.NewGuid()),
        new ExamRegistrationId(Guid.NewGuid()), new PersonId(Guid.NewGuid()), 1, officialReason,
        new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow);

    [Fact]
    public void Create_ShouldPreserveOfficialFailureReasonAsFinding()
    {
        ExamFailureAnalysis analysis = Create();
        Assert.Single(analysis.Findings);
        Assert.Equal(ExamFailureFindingKind.OfficialFailureReason, analysis.Findings.Single().Kind);
    }

    [Fact]
    public void Complete_ShouldRequireAtLeastOneFinding()
    {
        ExamFailureAnalysis analysis = Create(null);
        Assert.True(analysis.Complete("Summary", null, new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow).IsFailure);
    }

    [Fact]
    public void CompletedAnalysis_ShouldRejectNewFinding()
    {
        ExamFailureAnalysis analysis = Create();
        var actor = new UserId(Guid.NewGuid());
        Assert.True(analysis.Complete("Student must consolidate risk perception.", "Prepare remediation", actor, DateTimeOffset.UtcNow).IsSuccess);
        Assert.True(analysis.AddFinding(ExamFailureFindingKind.Weakness, "RiskPerception", null, false, "Instructor", actor, DateTimeOffset.UtcNow).IsFailure);
    }

    [Fact]
    public void SupersededAnalysis_ShouldRemainHistoricalAndRejectMutation()
    {
        ExamFailureAnalysis analysis = Create();
        analysis.Supersede(DateTimeOffset.UtcNow);
        Assert.Equal(ExamFailureAnalysisStatus.Superseded, analysis.Status);
        Assert.True(analysis.UpdateNarrative("x", null, null, new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow).IsFailure);
    }
}
