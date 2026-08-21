using DriveOS.Modules.TrainingDelivery.Application.Consequences;
using DriveOS.Modules.TrainingDelivery.Application.CancellationConsequences;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.CancellationConsequences;

internal sealed class TrainingSessionCancellationConsequenceMessage
{
    public Guid Id { get; set; }
    public OrganizationId OrganizationId { get; set; }
    public TrainingSessionId SessionId { get; set; }
    public SessionCancellationId CancellationId { get; set; }
    public TrainingSessionCancellationConsequenceKind Kind { get; set; }
    public TrainingSessionConsequenceStatus Status { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastAttemptAtUtc { get; set; }
    public DateTimeOffset? NextAttemptAtUtc { get; set; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
    public string? LastErrorCode { get; set; }
    public string? LastErrorDetail { get; set; }
}
