using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Activities.GetActivities;

public enum CrmActivityReadScope
{
    PublicOnly = 0,
    IncludeInternal = 1,
}

public sealed record GetLeadActivitiesQuery(
    OrganizationId OrganizationId,
    LeadId LeadId,
    CrmActivityReadScope Scope = CrmActivityReadScope.PublicOnly
) : IQuery<IReadOnlyList<CrmActivityResponse>>;

public sealed record GetRecentActivitiesQuery(
    OrganizationId OrganizationId,
    int Limit = 200,
    CrmActivityReadScope Scope = CrmActivityReadScope.PublicOnly
) : IQuery<IReadOnlyList<CrmActivityResponse>>;

public sealed record CrmActivityResponse(
    Guid Id,
    Guid LeadId,
    string Type,
    string Direction,
    string Subject,
    string? Details,
    DateTimeOffset OccurredAtUtc,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId
);
