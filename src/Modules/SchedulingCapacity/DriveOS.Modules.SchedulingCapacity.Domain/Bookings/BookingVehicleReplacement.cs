using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public sealed class BookingVehicleReplacement
{
    private BookingVehicleReplacement() { }

    internal BookingVehicleReplacement(BookingVehicleReplacementId id, BookingId bookingId, Guid operationId,
        Guid previousVehicleId, Guid replacementVehicleId, CalendarResourceId previousResourceId,
        CalendarResourceId replacementResourceId, VehicleReplacementMode mode, string reason, DateTimeOffset occurredAtUtc)
    {
        Id = id; BookingId = bookingId; OperationId = operationId; PreviousVehicleId = previousVehicleId;
        ReplacementVehicleId = replacementVehicleId; PreviousResourceId = previousResourceId;
        ReplacementResourceId = replacementResourceId; Mode = mode; Reason = reason; OccurredAtUtc = occurredAtUtc.ToUniversalTime();
    }

    public BookingVehicleReplacementId Id { get; private set; }
    public BookingId BookingId { get; private set; }
    public Guid OperationId { get; private set; }
    public Guid PreviousVehicleId { get; private set; }
    public Guid ReplacementVehicleId { get; private set; }
    public CalendarResourceId PreviousResourceId { get; private set; }
    public CalendarResourceId ReplacementResourceId { get; private set; }
    public VehicleReplacementMode Mode { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
