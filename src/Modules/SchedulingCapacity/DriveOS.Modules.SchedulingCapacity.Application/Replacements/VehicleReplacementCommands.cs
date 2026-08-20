using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.Replacements;

public sealed record ApplyVehicleReplacementCommand(OrganizationId OrganizationId, Guid OperationId, Guid PreviousVehicleId, Guid ReplacementVehicleId,
    int Mode, IReadOnlyCollection<BookingId> BookingIds, VehicleReplacementRequirements Requirements, string Reason) : ICommand<VehicleReplacementApplyResponse>;
