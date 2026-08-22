using DriveOS.Modules.ExamsCertification.Application.Success;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Success;

internal sealed class ExamSuccessConsequenceMessage
{
    public Guid Id { get; set; }
    public OrganizationId OrganizationId { get; set; }
    public ExamResultId ResultId { get; set; }
    public int ResultRevision { get; set; }
    public ExamSuccessConsequenceKind Kind { get; set; }
    public ExamSuccessConsequenceStatus Status { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastAttemptAtUtc { get; set; }
    public DateTimeOffset? NextAttemptAtUtc { get; set; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
    public DateTimeOffset? SupersededAtUtc { get; set; }
    public string? LastErrorCode { get; set; }
    public string? LastErrorDetail { get; set; }
}
