using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

/// <summary>Append-only request to correct or dispute a submitted session report. The original report value is always retained.</summary>
public sealed class SessionReportRevision : Entity<TrainingSessionReportRevisionId>
{
    private SessionReportRevision() { }
    private SessionReportRevision(TrainingSessionReportRevisionId id) : base(id) { }

    public TrainingSessionReportId SessionReportId { get; private set; }
    public Guid OperationId { get; private set; }
    public SessionReportRevisionScenario Scenario { get; private set; }
    public SessionReportRevisionStatus Status { get; private set; }
    public string FieldCode { get; private set; } = string.Empty;
    public string CurrentValue { get; private set; } = string.Empty;
    public string ProposedValue { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public bool HasFinancialImpact { get; private set; }
    public UserId RequestedByUserId { get; private set; }
    public DateTimeOffset RequestedAtUtc { get; private set; }
    public UserId? DecidedByUserId { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }
    public string? DecisionReason { get; private set; }
    public int? AppliedReportVersion { get; private set; }

    internal static Result<SessionReportRevision> Create(TrainingSessionReportRevisionId id, TrainingSessionReportId reportId, Guid operationId,
        SessionReportRevisionScenario scenario, string fieldCode, string currentValue, string proposedValue, string reason,
        bool hasFinancialImpact, bool approvalRequired, UserId actor, DateTimeOffset now)
    {
        if (id.IsEmpty || reportId.IsEmpty || operationId == Guid.Empty || actor.IsEmpty || string.IsNullOrWhiteSpace(fieldCode) ||
            string.IsNullOrWhiteSpace(proposedValue) || string.IsNullOrWhiteSpace(reason))
            return Result.Failure<SessionReportRevision>(TrainingSessionErrors.ReportRevisionInvalid);
        if (fieldCode.Trim().Length > 100 || currentValue.Length > 5000 || proposedValue.Length > 5000 || reason.Trim().Length > 2000)
            return Result.Failure<SessionReportRevision>(TrainingSessionErrors.ReportRevisionInvalid);
        return Result.Success(new SessionReportRevision(id)
        {
            SessionReportId = reportId,
            OperationId = operationId,
            Scenario = scenario,
            Status = hasFinancialImpact ? SessionReportRevisionStatus.PendingFinancialReview : approvalRequired ? SessionReportRevisionStatus.PendingApproval : SessionReportRevisionStatus.Pending,
            FieldCode = fieldCode.Trim(),
            CurrentValue = currentValue,
            ProposedValue = proposedValue,
            Reason = reason.Trim(),
            HasFinancialImpact = hasFinancialImpact,
            RequestedByUserId = actor,
            RequestedAtUtc = now.ToUniversalTime()
        });
    }

    internal Result Approve(UserId actor, DateTimeOffset now, int appliedVersion, string? decisionReason)
    {
        if (Status is SessionReportRevisionStatus.Approved or SessionReportRevisionStatus.Rejected or SessionReportRevisionStatus.ResolvedWithoutChange)
            return Result.Failure(TrainingSessionErrors.ReportRevisionAlreadyDecided);
        Status = SessionReportRevisionStatus.Approved;
        DecidedByUserId = actor;
        DecidedAtUtc = now.ToUniversalTime();
        DecisionReason = Normalize(decisionReason);
        AppliedReportVersion = appliedVersion;
        return Result.Success();
    }

    internal Result Reject(UserId actor, DateTimeOffset now, string decisionReason)
    {
        if (Status is SessionReportRevisionStatus.Approved or SessionReportRevisionStatus.Rejected or SessionReportRevisionStatus.ResolvedWithoutChange)
            return Result.Failure(TrainingSessionErrors.ReportRevisionAlreadyDecided);
        if (string.IsNullOrWhiteSpace(decisionReason)) return Result.Failure(TrainingSessionErrors.ReportRevisionDecisionReasonRequired);
        Status = SessionReportRevisionStatus.Rejected;
        DecidedByUserId = actor;
        DecidedAtUtc = now.ToUniversalTime();
        DecisionReason = Normalize(decisionReason);
        return Result.Success();
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().Length <= 2000 ? value.Trim() : value.Trim()[..2000];
}
