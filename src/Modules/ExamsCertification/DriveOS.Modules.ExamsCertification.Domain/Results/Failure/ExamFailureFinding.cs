using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Results.Failure;

public sealed class ExamFailureFinding
{
    private ExamFailureFinding() { }
    internal ExamFailureFinding(Guid id, ExamFailureFindingKind kind, string code, string? detail, bool critical, string source,
        UserId actorUserId, DateTimeOffset createdAtUtc)
    {
        Id = id;
        Kind = kind;
        Code = code;
        Detail = Normalize(detail);
        Critical = critical;
        Source = source.Trim();
        ActorUserId = actorUserId;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public ExamFailureFindingKind Kind { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string? Detail { get; private set; }
    public bool Critical { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public UserId ActorUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
