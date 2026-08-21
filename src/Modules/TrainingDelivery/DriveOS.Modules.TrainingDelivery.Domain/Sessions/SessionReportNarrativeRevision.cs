using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

/// <summary>
/// Immutable audit entry for one confidentiality-sensitive narrative field of a session report.
/// Previous values are never updated or deleted when a later version is saved.
/// </summary>
public sealed class SessionReportNarrativeRevision : Entity<Guid>
{
    private SessionReportNarrativeRevision() { }
    private SessionReportNarrativeRevision(Guid id) : base(id) { }

    public TrainingSessionReportId SessionReportId { get; private set; }
    public SessionReportNarrativeKind Kind { get; private set; }
    public int ReportVersion { get; private set; }
    public string? Content { get; private set; }
    public UserId ChangedByUserId { get; private set; }
    public DateTimeOffset ChangedAtUtc { get; private set; }

    internal static SessionReportNarrativeRevision Create(
        TrainingSessionReportId reportId,
        SessionReportNarrativeKind kind,
        int reportVersion,
        string? content,
        UserId actor,
        DateTimeOffset changedAtUtc) =>
        new(Guid.NewGuid())
        {
            SessionReportId = reportId,
            Kind = kind,
            ReportVersion = reportVersion,
            Content = content,
            ChangedByUserId = actor,
            ChangedAtUtc = changedAtUtc.ToUniversalTime()
        };
}
