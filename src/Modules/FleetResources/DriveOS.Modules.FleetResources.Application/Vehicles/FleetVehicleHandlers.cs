using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FleetResources.Application.Persistence;
using DriveOS.Modules.FleetResources.Domain.Vehicles;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FleetResources.Application.Vehicles;

public sealed class CreateFleetVehicleCommandHandler(IVehicleRepository repository, IFleetResourcesUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<CreateFleetVehicleCommand, DriveOS.SharedKernel.Identifiers.VehicleId>
{
    public async Task<Result<DriveOS.SharedKernel.Identifiers.VehicleId>> Handle(CreateFleetVehicleCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RegistrationNumber))
            return Result.Failure<DriveOS.SharedKernel.Identifiers.VehicleId>(VehicleErrors.RegistrationRequired);
        if (string.IsNullOrWhiteSpace(command.TransmissionType) || string.IsNullOrWhiteSpace(command.EnergyType) ||
            command.LicenseCategories is null || command.LicenseCategories.Count == 0)
            return Result.Failure<DriveOS.SharedKernel.Identifiers.VehicleId>(VehicleErrors.TechnicalProfileRequired);
        Vehicle? existing = await repository.FindByRegistrationAsync(command.OrganizationId, command.RegistrationNumber, cancellationToken);
        if (existing is not null)
            return existing.HasSameTechnicalIdentity(command.Vin, command.TransmissionType, command.EnergyType, command.DualControl,
                command.LicenseCategories, command.Adaptations)
                ? Result.Success(existing.Id)
                : Result.Failure<DriveOS.SharedKernel.Identifiers.VehicleId>(VehicleErrors.RegistrationConflict);
        Result<Vehicle> created = Vehicle.Create(command.VehicleId, command.OrganizationId, command.OwnerOrganizationId, command.BranchId,
            command.RegistrationNumber, command.Vin, command.Make, command.Model, command.TransmissionType, command.EnergyType, command.DualControl,
            command.LicenseCategories, command.Adaptations);
        if (created.IsFailure) return Result.Failure<DriveOS.SharedKernel.Identifiers.VehicleId>(created.Error);
        created.Value.SetCreatedAudit(clock.UtcNow, command.ActorUserId);
        repository.Add(created.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(created.Value.Id);
    }
}

public sealed class UpdateFleetVehicleComplianceCommandHandler(IVehicleRepository repository, IFleetResourcesUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<UpdateFleetVehicleComplianceCommand>
{
    public async Task<Result> Handle(UpdateFleetVehicleComplianceCommand command, CancellationToken cancellationToken)
    {
        Vehicle? vehicle = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.VehicleId, cancellationToken);
        if (vehicle is null) return Result.Failure(VehicleErrors.NotFound);
        Result result = vehicle.UpdateCompliance(command.TechnicalComplianceVerified, command.DocumentsCompliant, command.InsuranceValidUntilUtc,
            command.MaintenanceBlocking, command.NextMaintenanceDueAtUtc, command.OperationalStatus, command.BranchId, command.ProviderOrganizationId,
            command.Notes, clock.UtcNow, command.ActorUserId);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class RecordFleetVehicleOdometerCommandHandler(IVehicleRepository repository, IFleetResourcesUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<RecordFleetVehicleOdometerCommand>
{
    public async Task<Result> Handle(RecordFleetVehicleOdometerCommand command, CancellationToken cancellationToken)
    {
        Vehicle? vehicle = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.VehicleId, cancellationToken);
        if (vehicle is null) return Result.Failure(VehicleErrors.NotFound);
        Result result = vehicle.RecordOdometer(command.OdometerKilometers, command.RecordedAtUtc, clock.UtcNow, command.ActorUserId);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class GetFleetVehicleQueryHandler(IVehicleRepository repository) : IQueryHandler<GetFleetVehicleQuery, FleetVehicleResponse>
{
    public async Task<Result<FleetVehicleResponse>> Handle(GetFleetVehicleQuery query, CancellationToken cancellationToken)
    {
        Vehicle? vehicle = await repository.GetByIdAsync(query.OrganizationId, query.VehicleId, cancellationToken);
        return vehicle is null ? Result.Failure<FleetVehicleResponse>(VehicleErrors.NotFound) : Result.Success(Map(vehicle));
    }
    internal static FleetVehicleResponse Map(Vehicle x) => new(x.Id.Value, x.OrganizationId.Value, x.OwnerOrganizationId.Value,
        x.ProviderOrganizationId?.Value, x.BranchId?.Value, x.RegistrationNumber, x.Vin, x.Make, x.Model, x.TransmissionType, x.EnergyType,
        x.DualControl, Split(x.LicenseCategoriesCsv), Split(x.AdaptationsCsv), x.OperationalStatus.ToString(), x.TechnicalComplianceVerified,
        x.DocumentsCompliant, x.InsuranceValidUntilUtc, x.MaintenanceBlocking, x.NextMaintenanceDueAtUtc, x.LastComplianceVerifiedAtUtc,
        x.ComplianceNotes, x.CurrentOdometerKilometers, x.LastOdometerRecordedAtUtc);
    private static IReadOnlyCollection<string> Split(string value) => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

public sealed class GetFleetVehiclesQueryHandler(IVehicleRepository repository) : IQueryHandler<GetFleetVehiclesQuery, IReadOnlyList<FleetVehicleResponse>>
{
    public async Task<Result<IReadOnlyList<FleetVehicleResponse>>> Handle(GetFleetVehiclesQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<Vehicle> vehicles = await repository.ListAsync(query.OrganizationId, cancellationToken);
        return Result.Success<IReadOnlyList<FleetVehicleResponse>>(vehicles.Select(GetFleetVehicleQueryHandler.Map).ToArray());
    }
}
