using DriveOS.Modules.ExamsCertification.Domain.Remediation;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ExamsCertification.Remediation;

public sealed class ExamRemediationRequestTests
{
    [Fact]
    public void Create_without_training_path_is_deferred()
    {
        var r = ExamRemediationRequest.Create(new OrganizationId(Guid.NewGuid()), new ExamFailureAnalysisId(Guid.NewGuid()),
            new ExamResultId(Guid.NewGuid()), 2, new ExamAttemptId(Guid.NewGuid()), new ExamRegistrationId(Guid.NewGuid()),
            new PersonId(Guid.NewGuid()), 1, null, "Approved analysis", "Targeted remediation", [Guid.NewGuid()], ["TargetedSessions"],
            4, new UserId(Guid.NewGuid()), DateTimeOffset.UtcNow);
        Assert.True(r.IsSuccess);
        Assert.Equal(ExamRemediationRequestStatus.Deferred, r.Value.Status);
    }

    [Fact]
    public void Re_presentation_requires_completed_pedagogical_plan()
    {
        var now = DateTimeOffset.UtcNow;
        var r = ExamRemediationRequest.Create(new OrganizationId(Guid.NewGuid()), new ExamFailureAnalysisId(Guid.NewGuid()),
            new ExamResultId(Guid.NewGuid()), 1, new ExamAttemptId(Guid.NewGuid()), new ExamRegistrationId(Guid.NewGuid()),
            new PersonId(Guid.NewGuid()), 1, new TrainingPathId(Guid.NewGuid()), "Approved analysis", null, [Guid.NewGuid()], ["RemediationPlan"],
            3, new UserId(Guid.NewGuid()), now);
        Assert.True(r.Value.ValidateForRePresentation(new UserId(Guid.NewGuid()), now).IsFailure);
    }

    [Fact]
    public void Completed_plan_can_be_validated_for_new_presentation()
    {
        var now = DateTimeOffset.UtcNow;
        var actor = new UserId(Guid.NewGuid());
        var r = ExamRemediationRequest.Create(new OrganizationId(Guid.NewGuid()), new ExamFailureAnalysisId(Guid.NewGuid()),
            new ExamResultId(Guid.NewGuid()), 1, new ExamAttemptId(Guid.NewGuid()), new ExamRegistrationId(Guid.NewGuid()),
            new PersonId(Guid.NewGuid()), 1, new TrainingPathId(Guid.NewGuid()), "Approved analysis", null, [Guid.NewGuid()], ["RemediationPlan"],
            3, actor, now);
        Assert.True(r.Value.Configure(r.Value.TrainingPathId!.Value, actor, DateOnly.FromDateTime(now.UtcDateTime.AddDays(7)),
            DateOnly.FromDateTime(now.UtcDateTime.AddDays(14)), true, true, 3, actor, now).IsSuccess);
        Assert.True(r.Value.MockExamRequired);
        Assert.True(r.Value.FundingReviewRequired);
        Assert.Equal(DateOnly.FromDateTime(now.UtcDateTime.AddDays(14)), r.Value.TargetDate);
        Assert.True(r.Value.MarkProvisioning(actor, now).IsSuccess);
        r.Value.MarkPlanned(RemediationPlanId.New(), actor, now);
        r.Value.SynchronizePedagogicalStatus("Active", actor, now);
        r.Value.SynchronizePedagogicalStatus("Completed", actor, now);
        Assert.True(r.Value.ValidateForRePresentation(actor, now).IsSuccess);
        Assert.Equal(ExamRemediationRequestStatus.ValidatedForRePresentation, r.Value.Status);
    }
}
