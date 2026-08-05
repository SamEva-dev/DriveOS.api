using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.Models;

public sealed record OrganizationClosureModel(
    OrganizationClosureId Id,
    OrganizationId OrganizationId,
    OrganizationClosureReasonCode ReasonCode,
    string? ReasonDetails,
    DateTimeOffset RequestedEffectiveAtUtc,
    OrganizationDataDisposition DataDisposition,
    DateTimeOffset? RetentionUntilUtc,
    OrganizationClosureStatus Status,
    int Revision,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReviewedAtUtc,
    DateTimeOffset? ScheduledAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    string? DecisionComment)
{
    public static OrganizationClosureModel FromDomain(OrganizationClosure closure) => new(
        closure.Id, closure.OrganizationId, closure.ReasonCode, closure.ReasonDetails,
        closure.RequestedEffectiveAtUtc, closure.DataDisposition, closure.RetentionUntilUtc,
        closure.Status, closure.Revision, closure.CreatedAtUtc, closure.ReviewedAtUtc,
        closure.ScheduledAtUtc, closure.CompletedAtUtc, closure.CancelledAtUtc,
        closure.DecisionComment);
}
