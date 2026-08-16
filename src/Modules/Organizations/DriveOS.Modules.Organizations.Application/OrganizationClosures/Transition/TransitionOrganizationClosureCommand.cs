using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.OrganizationClosures;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.Transition;

public sealed record TransitionOrganizationClosureCommand(
    OrganizationClosureId ClosureId,
    OrganizationClosureAction Action,
    string? Comment,
    DateTimeOffset? ScheduledAtUtc
) : ICommand;
