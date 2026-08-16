using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Assessments.Events;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Modules.CRM.Assessments;

public sealed class AssessmentSessionTests
{
    private static readonly OrganizationId OrganizationId = new(Guid.NewGuid());
    private static readonly UserId EvaluatorId = new(Guid.NewGuid());

    [Fact]
    public void Start_ShouldSnapshotQuestionnaireAndRaiseEvent()
    {
        var result = AssessmentSession.Start(
            AssessmentSessionId.New(),
            OrganizationId,
            AssessmentAppointmentId.New(),
            new LeadId(Guid.NewGuid()),
            EvaluatorId,
            "FR-B-INITIAL",
            3,
            "{\"sections\":[]}",
            DateTimeOffset.UtcNow
        );

        Assert.True(result.IsSuccess);
        Assert.Equal(AssessmentSessionStatus.InProgress, result.Value.Status);
        Assert.Equal(1, result.Value.Revision);
        Assert.Contains(result.Value.DomainEvents, x => x is InitialAssessmentStartedDomainEvent);
    }

    [Fact]
    public void SaveDraft_ShouldIncrementRevisionAndKeepNotesSeparated()
    {
        AssessmentSession session = CreateSession();
        var result = session.SaveDraft(
            "[{\"questionId\":\"observation\",\"value\":true}]",
            "fact",
            "interpretation",
            "recommendation",
            "internal",
            "visible",
            true,
            DateTimeOffset.UtcNow
        );

        Assert.True(result.IsSuccess);
        Assert.Equal(AssessmentSessionStatus.DraftCompleted, session.Status);
        Assert.Equal(2, session.Revision);
        Assert.Equal("internal", session.InternalNotes);
        Assert.Equal("visible", session.ProspectComment);
    }

    [Fact]
    public void Submit_ShouldMakeSessionImmutable()
    {
        AssessmentSession session = CreateSession();
        session.SaveDraft(
            "[{\"questionId\":\"q1\",\"value\":3}]",
            null,
            null,
            null,
            null,
            null,
            true,
            DateTimeOffset.UtcNow
        );
        Assert.True(session.Submit(EvaluatorId, DateTimeOffset.UtcNow).IsSuccess);

        var secondSave = session.SaveDraft(
            "[]",
            null,
            null,
            null,
            null,
            null,
            false,
            DateTimeOffset.UtcNow
        );
        Assert.True(secondSave.IsFailure);
        Assert.Equal(AssessmentSessionStatus.Submitted, session.Status);
        Assert.Contains(session.DomainEvents, x => x is InitialAssessmentSubmittedDomainEvent);
    }

    [Fact]
    public void Result_requires_submitted_assessment()
    {
        AssessmentSession session = CreateSession();

        var result = session.SaveResult(
            "{\"summary\":\"ready\"}",
            AssessmentResultConfidence.Medium,
            null,
            EvaluatorId,
            DateTimeOffset.UtcNow
        );

        Assert.True(result.IsFailure);
        Assert.Equal(AssessmentResultStatus.None, session.ResultStatus);
    }

    [Fact]
    public void Validated_result_is_versioned_immutable_and_shareable()
    {
        AssessmentSession session = CreateSubmittedSession();
        Assert.True(
            session
                .SaveResult(
                    "{\"summary\":\"Permis B automatique\",\"practicalHours\":{\"min\":20,\"max\":26}}",
                    AssessmentResultConfidence.Medium,
                    "{\"source\":\"DriveOSAI\",\"explanation\":[\"score\"]}",
                    EvaluatorId,
                    DateTimeOffset.UtcNow
                )
                .IsSuccess
        );

        Assert.True(session.ValidateResult(EvaluatorId, DateTimeOffset.UtcNow).IsSuccess);
        Assert.True(
            session
                .SaveResult(
                    "{\"summary\":\"changed\"}",
                    AssessmentResultConfidence.High,
                    null,
                    EvaluatorId,
                    DateTimeOffset.UtcNow
                )
                .IsFailure
        );
        Assert.True(session.MarkResultShared(EvaluatorId, DateTimeOffset.UtcNow).IsSuccess);
        Assert.Equal(AssessmentResultStatus.Shared, session.ResultStatus);
        Assert.Contains(
            session.DomainEvents,
            x => x is InitialAssessmentResultValidatedDomainEvent
        );
        Assert.Contains(session.DomainEvents, x => x is InitialAssessmentResultSharedDomainEvent);
    }

    private static AssessmentSession CreateSession() =>
        AssessmentSession
            .Start(
                AssessmentSessionId.New(),
                OrganizationId,
                AssessmentAppointmentId.New(),
                new LeadId(Guid.NewGuid()),
                EvaluatorId,
                "FR-B-INITIAL",
                1,
                "{\"sections\":[]}",
                DateTimeOffset.UtcNow
            )
            .Value;

    private static AssessmentSession CreateSubmittedSession()
    {
        AssessmentSession session = CreateSession();
        session.SaveDraft(
            "[{\"questionId\":\"q1\",\"value\":3}]",
            null,
            null,
            null,
            null,
            null,
            true,
            DateTimeOffset.UtcNow
        );
        session.Submit(EvaluatorId, DateTimeOffset.UtcNow);
        return session;
    }
}
