using DomainRelay.Abstractions;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.CRM.Application.Dashboard.GetDashboard;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Api.Endpoints.Crm;

public static class CrmDashboardEndpoints
{
    public static IEndpointRouteBuilder MapCrmDashboardEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/crm/dashboard", GetAsync)
            .WithTags("CRM - Dashboard")
            .WithName("GetCrmDashboard")
            .WithSummary("Obtenir le tableau de bord CRM agrégé")
            .Produces<CrmDashboardResponse>()
            .RequireAuthorization("Crm.Dashboard.Read");
        return endpoints;
    }

    private static async Task<IResult> GetAsync(string? scope, Guid? branchId,
        DateTimeOffset? fromUtc, DateTimeOffset? toUtc, Guid? assignedAdvisorId,
        string? source, string? status,
        IMediator mediator, ICurrentTenant tenant, IAuthorizationService authorization,
        OrganizationsDbContext organizationsDbContext, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);

        string normalizedScope = string.IsNullOrWhiteSpace(scope) ? "organization" : scope.Trim().ToLowerInvariant();
        if (normalizedScope is not ("branch" or "organization" or "network"))
            return Results.BadRequest(new { key = "Crm.Dashboard.Scope.Unsupported",
                messageKey = "errors.crm.dashboard.scopeUnsupported" });
        if (normalizedScope == "branch" && (!branchId.HasValue || branchId == Guid.Empty))
            return Results.BadRequest(new { key = "Crm.Dashboard.BranchId.Required",
                messageKey = "errors.crm.dashboard.branchRequired" });
        if (fromUtc.HasValue && toUtc.HasValue && fromUtc.Value >= toUtc.Value)
            return Results.BadRequest(new { key = "Crm.Dashboard.Period.Invalid",
                messageKey = "errors.crm.dashboard.periodInvalid" });
        if (assignedAdvisorId == Guid.Empty)
            return Results.BadRequest(new { key = "Crm.Dashboard.Advisor.Invalid",
                messageKey = "errors.crm.dashboard.advisorInvalid" });
        if (!string.IsNullOrWhiteSpace(source)
            && !Enum.TryParse<LeadSourceType>(source, true, out _))
            return Results.BadRequest(new { key = "Crm.Dashboard.Source.Unsupported",
                messageKey = "errors.crm.dashboard.sourceUnsupported" });
        if (!string.IsNullOrWhiteSpace(status)
            && !Enum.TryParse<LeadStatus>(status, true, out _))
            return Results.BadRequest(new { key = "Crm.Dashboard.Status.Unsupported",
                messageKey = "errors.crm.dashboard.statusUnsupported" });

        string scopePermission = normalizedScope switch
        {
            "branch" => "Crm.Dashboard.Scope.Branch",
            "network" => "Crm.Dashboard.Scope.Network",
            _ => "Crm.Dashboard.Scope.Organization"
        };
        AuthorizationResult scopeAuthorization = await authorization.AuthorizeAsync(
            context.User, null, scopePermission);
        if (!scopeAuthorization.Succeeded)
            return Results.Forbid();

        var branchRows = await organizationsDbContext.Branches
    .AsNoTracking()
    .Where(branch =>
        branch.OrganizationId == tenant.OrganizationId.Value
        && branch.Status != BranchStatus.Closed)
    .OrderByDescending(branch => branch.IsPrimary)
    .ThenBy(branch => branch.NormalizedName)
    .Select(branch => new
    {
        branch.Id,
        branch.Name,
        branch.Code,
        branch.IsPrimary
    })
    .ToArrayAsync(ct);

        CrmDashboardBranchScope[] availableBranches = branchRows
            .Select(branch => new CrmDashboardBranchScope(
                branch.Id.Value,
                branch.Name.Value,
                branch.Code.Value,
                branch.IsPrimary))
            .ToArray();

        if (normalizedScope == "branch"
            && !availableBranches.Any(x => x.Id == branchId!.Value))
            return Results.BadRequest(new { key = "Crm.Dashboard.Branch.NotAvailable",
                messageKey = "errors.crm.dashboard.branchNotAvailable" });

        var currentOrganizationId = tenant.OrganizationId.Value;
        OrganizationId[] scopedOrganizationIds = [currentOrganizationId];
        CrmDashboardOrganizationScope[] includedOrganizations = [];

        if (normalizedScope == "network")
        {
            var currentOrganization = await organizationsDbContext.Organizations.AsNoTracking()
                .Where(x => x.Id == currentOrganizationId)
                .Select(x => new { x.Id, x.Type, x.Status, x.LegalName })
                .SingleOrDefaultAsync(ct);
            if (currentOrganization is null)
                return Results.BadRequest(new { key = "Crm.Dashboard.Network.CurrentOrganizationNotFound",
                    messageKey = "errors.crm.dashboard.networkCurrentOrganizationNotFound" });

            OrganizationId? networkOrganizationId = currentOrganization.Type == OrganizationType.DrivingSchoolNetwork
                ? currentOrganization.Id
                : await organizationsDbContext.NetworkOrganizationMemberships.AsNoTracking()
                    .Where(x => x.MemberOrganizationId == currentOrganizationId && x.EndedAtUtc == null)
                    .Select(x => (OrganizationId?)x.NetworkOrganizationId)
                    .SingleOrDefaultAsync(ct);

            if (!networkOrganizationId.HasValue)
                return Results.BadRequest(new { key = "Crm.Dashboard.Network.NotAvailable",
                    messageKey = "errors.crm.dashboard.networkNotAvailable" });

            OrganizationId[] memberIds = await organizationsDbContext.NetworkOrganizationMemberships.AsNoTracking()
                .Where(x => x.NetworkOrganizationId == networkOrganizationId.Value && x.EndedAtUtc == null)
                .Select(x => x.MemberOrganizationId)
                .ToArrayAsync(ct);
            scopedOrganizationIds = memberIds.Length == 0 ? [networkOrganizationId.Value] : memberIds;

            includedOrganizations = await organizationsDbContext.Organizations.AsNoTracking()
                .Where(x => scopedOrganizationIds.Contains(x.Id) && x.Status != OrganizationStatus.Closed)
                .OrderBy(x => x.LegalName)
                .Select(x => new CrmDashboardOrganizationScope(x.Id.Value, x.LegalName,
                    x.Type == OrganizationType.DrivingSchoolNetwork))
                .ToArrayAsync(ct);
            scopedOrganizationIds = includedOrganizations.Select(x => new OrganizationId(x.Id)).ToArray();
        }

        var filters = new CrmDashboardFilters(
            fromUtc?.ToUniversalTime(),
            toUtc?.ToUniversalTime(),
            assignedAdvisorId.HasValue ? new UserId(assignedAdvisorId.Value) : null,
            string.IsNullOrWhiteSpace(source) ? null : Enum.Parse<LeadSourceType>(source, true),
            string.IsNullOrWhiteSpace(status) ? null : Enum.Parse<LeadStatus>(status, true));
        Result<CrmDashboardResponse> result = await mediator.Send(new GetCrmDashboardQuery(
            scopedOrganizationIds, normalizedScope, branchId, filters), ct);
        return result.IsFailure
            ? result.Error.ToHttpResult(context)
            : Results.Ok(result.Value with
            {
                AvailableBranches = availableBranches,
                IncludedOrganizations = includedOrganizations
            });
    }
}
