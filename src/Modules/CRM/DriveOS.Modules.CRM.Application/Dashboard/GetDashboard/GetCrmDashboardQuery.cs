using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Dashboard.GetDashboard;

public sealed record GetCrmDashboardQuery(IReadOnlyCollection<OrganizationId> OrganizationIds,
    string Scope, Guid? BranchId, CrmDashboardFilters Filters) : IQuery<CrmDashboardResponse>;
