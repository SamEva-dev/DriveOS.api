using DriveOS.Application.Abstractions.Messaging;

namespace DriveOS.Modules.Organizations.Application.Organizations.OrganizationStatusHistory;

public sealed record GetOrganizationStatusHistoryQuery(Guid OrganizationId)
    : IQuery<IReadOnlyList<OrganizationStatusHistoryItem>>;
