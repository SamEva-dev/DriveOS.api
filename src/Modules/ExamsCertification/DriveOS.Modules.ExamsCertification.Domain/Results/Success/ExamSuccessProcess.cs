using DriveOS.Modules.ExamsCertification.Domain.Results.Success.Events;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Results.Success;

/// <summary>
/// Auditable business view of the consequences of one finalized passing result revision.
/// It never owns pedagogy, contracts, finance, certification, communication or scheduling data;
/// action states mirror outcomes returned by those authoritative bounded contexts.
/// </summary>
public sealed class ExamSuccessProcess : AggregateRoot<ExamSuccessProcessId>
{
    private readonly List<ExamSuccessAction> _actions = [];
    private ExamSuccessProcess() { }
    private ExamSuccessProcess(ExamSuccessProcessId id, OrganizationId organizationId, ExamResultId resultId, int resultRevision,
        ExamAttemptId attemptId, ExamRegistrationId registrationId, PersonId studentId, int attemptNumber, UserId actor, DateTimeOffset now) : base(id)
    {
        OrganizationId = organizationId; ExamResultId = resultId; ResultRevision = resultRevision; AttemptId = attemptId;
        RegistrationId = registrationId; StudentId = studentId; AttemptNumber = attemptNumber;
        Status = ExamSuccessProcessStatus.PassedPendingClosure; CreatedAtUtc = now.ToUniversalTime(); CreatedByUserId = actor;
        Add(ExamSuccessActionCode.ClosePedagogicalPath, true);
        Add(ExamSuccessActionCode.CloseTrainingContract, true);
        Add(ExamSuccessActionCode.CheckFinancialSituation, false);
        Add(ExamSuccessActionCode.PrepareCertification, false);
        Add(ExamSuccessActionCode.UpdateStudentJourney, false);
        Add(ExamSuccessActionCode.ReviewFutureScheduling, false);
        Add(ExamSuccessActionCode.NotifyStudent, false);
        Add(ExamSuccessActionCode.RequestSatisfaction, false);
        Add(ExamSuccessActionCode.PrepareArchive, false);
        Add(ExamSuccessActionCode.ProposeNextTraining, false);
        Add(ExamSuccessActionCode.PublishAnalytics, false);
    }

    public OrganizationId OrganizationId { get; private set; }
    public ExamResultId ExamResultId { get; private set; }
    public int ResultRevision { get; private set; }
    public ExamAttemptId AttemptId { get; private set; }
    public ExamRegistrationId RegistrationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public int AttemptNumber { get; private set; }
    public ExamSuccessProcessStatus Status { get; private set; }
    public IReadOnlyCollection<ExamSuccessAction> Actions => _actions;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public UserId? CompletedByUserId { get; private set; }
    public DateTimeOffset? SupersededAtUtc { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }
    public UserId? ArchivedByUserId { get; private set; }

    public static ExamSuccessProcess Create(OrganizationId organizationId, ExamResultId resultId, int resultRevision, ExamAttemptId attemptId,
        ExamRegistrationId registrationId, PersonId studentId, int attemptNumber, UserId actor, DateTimeOffset now)
    {
        var process = new ExamSuccessProcess(ExamSuccessProcessId.New(), organizationId, resultId, resultRevision, attemptId, registrationId, studentId, attemptNumber, actor, now);
        process.RaiseDomainEvent(new ExamSuccessProcessStartedDomainEvent(process.Id, organizationId, resultId, resultRevision, attemptId, registrationId, studentId));
        return process;
    }

    public Result ApplyConsequence(ExamSuccessActionCode code, ExamSuccessActionStatus status, string? evidenceReference, string? reasonCode, string? detail, UserId? actor, DateTimeOffset now)
    {
        if (Status == ExamSuccessProcessStatus.Superseded) return Result.Failure(ExamSuccessProcessErrors.Superseded);
        ExamSuccessAction? action = _actions.SingleOrDefault(x => x.Code == code);
        if (action is null) return Result.Failure(ExamSuccessProcessErrors.ActionNotFound);
        action.Apply(status, evidenceReference, reasonCode, detail, actor, now);
        RaiseDomainEvent(new ExamSuccessActionStateChangedDomainEvent(Id, ExamResultId, code, status));
        RefreshStatus();
        return Result.Success();
    }

    public Result Complete(UserId actor, DateTimeOffset now)
    {
        if (Status == ExamSuccessProcessStatus.Superseded) return Result.Failure(ExamSuccessProcessErrors.Superseded);
        if (_actions.Any(x => x.Blocking && x.Status is not (ExamSuccessActionStatus.Completed or ExamSuccessActionStatus.NotApplicable)))
            return Result.Failure(ExamSuccessProcessErrors.BlockingActionsRemain);
        Status = ExamSuccessProcessStatus.Completed; CompletedAtUtc = now.ToUniversalTime(); CompletedByUserId = actor;
        RaiseDomainEvent(new ExamSuccessProcessCompletedDomainEvent(Id, OrganizationId, ExamResultId, StudentId));
        return Result.Success();
    }

    public void Supersede(DateTimeOffset now)
    {
        if (Status == ExamSuccessProcessStatus.Superseded) return;
        Status = ExamSuccessProcessStatus.Superseded; SupersededAtUtc = now.ToUniversalTime();
        foreach (ExamSuccessAction action in _actions.Where(x => x.Status is not ExamSuccessActionStatus.Superseded))
            action.Apply(ExamSuccessActionStatus.Superseded, action.EvidenceReference, "exam-success.finalization-superseded", action.Detail, null, now);
        RaiseDomainEvent(new ExamSuccessProcessSupersededDomainEvent(Id, OrganizationId, ExamResultId, ResultRevision));
    }

    public Result Archive(UserId actor, DateTimeOffset now)
    {
        if (Status != ExamSuccessProcessStatus.Completed) return Result.Failure(ExamSuccessProcessErrors.BlockingActionsRemain);
        Status = ExamSuccessProcessStatus.Archived; ArchivedAtUtc = now.ToUniversalTime(); ArchivedByUserId = actor; return Result.Success();
    }

    private void Add(ExamSuccessActionCode code, bool blocking) => _actions.Add(new ExamSuccessAction(code, blocking));
    private void RefreshStatus()
    {
        if (Status is ExamSuccessProcessStatus.Completed or ExamSuccessProcessStatus.Archived or ExamSuccessProcessStatus.Superseded) return;
        Status = _actions.All(x => !x.Blocking || x.Status is ExamSuccessActionStatus.Completed or ExamSuccessActionStatus.NotApplicable)
            ? ExamSuccessProcessStatus.ReadyToComplete : ExamSuccessProcessStatus.PassedPendingClosure;
    }
}
