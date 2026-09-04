namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Outbox;

public sealed class MarketplaceOutboxMessage
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; set; }
    public DateTimeOffset EnqueuedAtUtc { get; set; }
    public DateTimeOffset NextAttemptAtUtc { get; set; }
    public string Status { get; set; } = "Pending";
    public int AttemptCount { get; set; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
    public string? LastErrorCode { get; set; }
}
