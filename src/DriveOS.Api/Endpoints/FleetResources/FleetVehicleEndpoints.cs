using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.FleetResources.Application.Vehicles;
using DriveOS.Modules.FleetResources.Domain.Vehicles;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Security.Contracts;

namespace DriveOS.Api.Endpoints.FleetResources;

internal static class FleetVehicleEndpoints
{
    internal static IEndpointRouteBuilder MapFleetVehicleEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/fleet/vehicles").WithTags("Fleet - Vehicles");
        group.MapGet("/", GetVehicles).RequireAuthorization(DriveOsPermissionCodes.Fleet.VehiclesRead);
        group.MapGet("/{vehicleId:guid}", GetVehicle).RequireAuthorization(DriveOsPermissionCodes.Fleet.VehiclesRead);
        group.MapPost("/", CreateVehicle).RequireAuthorization(DriveOsPermissionCodes.Fleet.VehiclesManage);
        group.MapPut("/{vehicleId:guid}/compliance", UpdateCompliance).RequireAuthorization(DriveOsPermissionCodes.Fleet.VehiclesManageCompliance);
        group.MapPut("/{vehicleId:guid}/odometer", RecordOdometer).RequireAuthorization(DriveOsPermissionCodes.Fleet.VehiclesManage);
        return app;
    }

    private static async Task<IResult> GetVehicles(IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org) return Results.Unauthorized();
        Result<IReadOnlyList<FleetVehicleResponse>> r = await mediator.Send(new GetFleetVehiclesQuery(org), ct);
        return r.IsSuccess ? Results.Ok(r.Value) : ToProblem(r.Error);
    }

    private static async Task<IResult> GetVehicle(Guid vehicleId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org) return Results.Unauthorized();
        Result<FleetVehicleResponse> r = await mediator.Send(new GetFleetVehicleQuery(org, new VehicleId(vehicleId)), ct);
        return r.IsSuccess ? Results.Ok(r.Value) : ToProblem(r.Error);
    }

    private static async Task<IResult> CreateVehicle(CreateFleetVehicleRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        VehicleId id = request.VehicleId is { } raw && raw != Guid.Empty ? new VehicleId(raw) : VehicleId.New();
        Result<VehicleId> r = await mediator.Send(new CreateFleetVehicleCommand(org, id, request.OwnerOrganizationId is { } owner && owner != Guid.Empty ? new OrganizationId(owner) : org,
            request.BranchId is { } branch ? new BranchId(branch) : null, request.RegistrationNumber, request.Vin, request.Make, request.Model,
            request.TransmissionType, request.EnergyType, request.DualControl, request.LicenseCategories, request.Adaptations ?? [], actor), ct);
        return r.IsSuccess ? Results.Created($"/api/fleet/vehicles/{r.Value.Value}", new { id = r.Value.Value }) : ToProblem(r.Error);
    }

    private static async Task<IResult> UpdateCompliance(Guid vehicleId, UpdateFleetVehicleComplianceRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        if (!Enum.TryParse<VehicleOperationalStatus>(request.OperationalStatus, true, out var status))
            return Results.BadRequest(new { code = "Fleet.Vehicle.InvalidOperationalStatus", messageKey = "errors.fleet.vehicle.invalidOperationalStatus" });
        Result r = await mediator.Send(new UpdateFleetVehicleComplianceCommand(org, new VehicleId(vehicleId), request.TechnicalComplianceVerified,
            request.DocumentsCompliant, request.InsuranceValidUntilUtc, request.MaintenanceBlocking, request.NextMaintenanceDueAtUtc, status,
            request.BranchId is { } branch ? new BranchId(branch) : null, request.ProviderOrganizationId is { } provider ? new OrganizationId(provider) : null, request.Notes, actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }

    private static async Task<IResult> RecordOdometer(Guid vehicleId, RecordFleetVehicleOdometerRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } org || user.UserId is not { } actor) return Results.Unauthorized();
        Result result = await mediator.Send(new RecordFleetVehicleOdometerCommand(org, new VehicleId(vehicleId),
            request.OdometerKilometers, request.RecordedAtUtc, actor), ct);
        return result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);
    }

    private static IResult ToProblem(Error error) => Results.Problem(statusCode: error.Type switch
    { ErrorType.NotFound => 404, ErrorType.Conflict => 409, ErrorType.Validation => 400, _ => 400 },
        extensions: new Dictionary<string, object?> { ["code"] = error.Code, ["messageKey"] = error.MessageKey });
}

public sealed record CreateFleetVehicleRequest(Guid? VehicleId, Guid? OwnerOrganizationId, Guid? BranchId, string RegistrationNumber, string? Vin,
    string Make, string Model, string TransmissionType, string EnergyType, bool DualControl, IReadOnlyCollection<string> LicenseCategories, IReadOnlyCollection<string>? Adaptations);
public sealed record UpdateFleetVehicleComplianceRequest(bool TechnicalComplianceVerified, bool DocumentsCompliant, DateTimeOffset? InsuranceValidUntilUtc,
    bool MaintenanceBlocking, DateTimeOffset? NextMaintenanceDueAtUtc, string OperationalStatus, Guid? BranchId, Guid? ProviderOrganizationId, string? Notes);
public sealed record RecordFleetVehicleOdometerRequest(long OdometerKilometers, DateTimeOffset RecordedAtUtc);
