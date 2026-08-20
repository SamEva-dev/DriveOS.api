using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Application.Replacements;

public sealed class ApplyVehicleReplacementCommandHandler(IVehicleReplacementService service)
    : ICommandHandler<ApplyVehicleReplacementCommand, VehicleReplacementApplyResponse>
{
    public Task<Result<VehicleReplacementApplyResponse>> Handle(ApplyVehicleReplacementCommand command, CancellationToken cancellationToken) =>
        service.ApplyAsync(command.OrganizationId, command.OperationId, command.PreviousVehicleId, command.ReplacementVehicleId,
            command.Mode, command.BookingIds, command.Requirements, command.Reason, cancellationToken);
}
