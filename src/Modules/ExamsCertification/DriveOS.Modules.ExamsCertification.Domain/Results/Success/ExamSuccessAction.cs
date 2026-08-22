using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Results.Success;

/// <summary>Visible business projection of one post-success consequence. The authoritative state is driven by the owning BC integration.</summary>
public sealed class ExamSuccessAction
{
    private ExamSuccessAction() { }
    internal ExamSuccessAction(ExamSuccessActionCode code, bool blocking)
    {
        Id = Guid.NewGuid(); Code = code; Blocking = blocking; Status = ExamSuccessActionStatus.Pending;
    }

    public Guid Id { get; private set; }
    public ExamSuccessActionCode Code { get; private set; }
    public bool Blocking { get; private set; }
    public ExamSuccessActionStatus Status { get; private set; }
    public string? EvidenceReference { get; private set; }
    public string? ReasonCode { get; private set; }
    public string? Detail { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public UserId? UpdatedByUserId { get; private set; }

    internal void Apply(ExamSuccessActionStatus status, string? evidenceReference, string? reasonCode, string? detail, UserId? actor, DateTimeOffset now)
    {
        Status = status;
        EvidenceReference = string.IsNullOrWhiteSpace(evidenceReference) ? null : evidenceReference.Trim();
        ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? null : reasonCode.Trim();
        Detail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim();
        UpdatedAtUtc = now.ToUniversalTime();
        UpdatedByUserId = actor;
    }
}
