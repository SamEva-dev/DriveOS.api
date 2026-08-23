using DomainRelay.Abstractions;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Organizations.Application.RegulatoryIntegrations;
using DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.Organization.RegulatoryIntegrations;

public sealed record ConfigureRegulatoryIntegrationRequest(
    Guid? BranchId,
    string CountryCode,
    string ProviderCode,
    string ExternalAccountReference,
    string? SecretReference);

public sealed record UpdateRegulatoryIntegrationRequest(string ExternalAccountReference, string? SecretReference, int ExpectedRevision);
public sealed record ChangeRegulatoryIntegrationStatusRequest(string Status, int ExpectedRevision);

public static class RegulatoryIntegrationEndpoints
{
    public static IEndpointRouteBuilder MapRegulatoryIntegrationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/organizations/{organizationId:guid}/regulatory-integrations").WithTags("Organization regulatory integrations");
        group.MapGet("/", GetAsync).RequireAuthorization(DriveOsPermissionCodes.RegulatoryIntegrations.Read);
        group.MapPost("/", ConfigureAsync).RequireAuthorization(DriveOsPermissionCodes.RegulatoryIntegrations.Manage);
        group.MapPut("/{connectionId:guid}", UpdateAsync).RequireAuthorization(DriveOsPermissionCodes.RegulatoryIntegrations.Manage);
        group.MapPut("/{connectionId:guid}/status", ChangeStatusAsync).RequireAuthorization(DriveOsPermissionCodes.RegulatoryIntegrations.Manage);
        return endpoints;
    }

    private static async Task<IResult> GetAsync(Guid organizationId, IMediator mediator, ICurrentTenant tenant, HttpContext http, CancellationToken ct)
    {
        OrganizationId id = new(organizationId);
        if (organizationId == Guid.Empty) return RegulatoryIntegrationConnectionErrors.NotFound.ToHttpResult(http);
        if (tenant.HasTenant && tenant.OrganizationId != id) return Results.Forbid();
        Result<IReadOnlyList<RegulatoryIntegrationConnectionResponse>> result = await mediator.Send(new GetRegulatoryIntegrationConnectionsQuery(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToHttpResult(http);
    }

    private static async Task<IResult> ConfigureAsync(Guid organizationId, ConfigureRegulatoryIntegrationRequest request, IMediator mediator, ICurrentTenant tenant, HttpContext http, CancellationToken ct)
    {
        OrganizationId id = new(organizationId);
        if (organizationId == Guid.Empty) return RegulatoryIntegrationConnectionErrors.NotFound.ToHttpResult(http);
        if (tenant.HasTenant && tenant.OrganizationId != id) return Results.Forbid();
        Result<RegulatoryIntegrationConnectionId> result = await mediator.Send(new ConfigureRegulatoryIntegrationConnectionCommand(
            id,
            request.BranchId.HasValue ? new BranchId(request.BranchId.Value) : null,
            request.CountryCode,
            request.ProviderCode,
            request.ExternalAccountReference,
            request.SecretReference), ct);
        return result.IsSuccess
            ? Results.Created($"/api/organizations/{organizationId}/regulatory-integrations/{result.Value.Value}", new { id = result.Value.Value })
            : result.Error.ToHttpResult(http);
    }

    private static async Task<IResult> UpdateAsync(Guid organizationId, Guid connectionId, UpdateRegulatoryIntegrationRequest request, IMediator mediator, ICurrentTenant tenant, HttpContext http, CancellationToken ct)
    {
        OrganizationId id = new(organizationId);
        if (tenant.HasTenant && tenant.OrganizationId != id) return Results.Forbid();
        Result result = await mediator.Send(new UpdateRegulatoryIntegrationConnectionCommand(id, new RegulatoryIntegrationConnectionId(connectionId), request.ExternalAccountReference, request.SecretReference, request.ExpectedRevision), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(http);
    }

    private static async Task<IResult> ChangeStatusAsync(Guid organizationId, Guid connectionId, ChangeRegulatoryIntegrationStatusRequest request, IMediator mediator, ICurrentTenant tenant, HttpContext http, CancellationToken ct)
    {
        OrganizationId id = new(organizationId);
        if (tenant.HasTenant && tenant.OrganizationId != id) return Results.Forbid();
        if (!Enum.TryParse(request.Status, true, out RegulatoryIntegrationConnectionStatus status))
            return Results.BadRequest(new { code = "RegulatoryIntegrations.Status.Invalid", messageKey = "errors.regulatoryIntegrations.status.invalid" });
        Result result = await mediator.Send(new ChangeRegulatoryIntegrationConnectionStatusCommand(id, new RegulatoryIntegrationConnectionId(connectionId), status, request.ExpectedRevision), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(http);
    }
}
