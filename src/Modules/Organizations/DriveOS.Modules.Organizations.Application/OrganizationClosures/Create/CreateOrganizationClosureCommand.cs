using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.Create;

public sealed record CreateOrganizationClosureCommand(
    OrganizationId OrganizationId,
    OrganizationClosureReasonCode ReasonCode,
    string? ReasonDetails,
    DateTimeOffset RequestedEffectiveAtUtc,
    OrganizationDataDisposition DataDisposition,
    DateTimeOffset? RetentionUntilUtc
) : ICommand<OrganizationClosureId>;
