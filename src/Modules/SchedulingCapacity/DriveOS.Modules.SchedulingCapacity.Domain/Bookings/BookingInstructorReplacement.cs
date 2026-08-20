using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public sealed class BookingInstructorReplacement
{
    private BookingInstructorReplacement() { }

    internal BookingInstructorReplacement(
        BookingInstructorReplacementId id,
        BookingId bookingId,
        Guid operationId,
        UserId previousInstructorId,
        UserId replacementInstructorId,
        CalendarResourceId previousResourceId,
        CalendarResourceId replacementResourceId,
        InstructorReplacementMode mode,
        string reason,
        DateTimeOffset occurredAtUtc,
        DateTimeOffset? accessExpiresAtUtc)
    {
        Id = id;
        BookingId = bookingId;
        OperationId = operationId;
        PreviousInstructorId = previousInstructorId;
        ReplacementInstructorId = replacementInstructorId;
        PreviousResourceId = previousResourceId;
        ReplacementResourceId = replacementResourceId;
        Mode = mode;
        Reason = reason;
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
        AccessExpiresAtUtc = accessExpiresAtUtc?.ToUniversalTime();
    }

    public BookingInstructorReplacementId Id { get; private set; }
    public BookingId BookingId { get; private set; }
    public Guid OperationId { get; private set; }
    public UserId PreviousInstructorId { get; private set; }
    public UserId ReplacementInstructorId { get; private set; }
    public CalendarResourceId PreviousResourceId { get; private set; }
    public CalendarResourceId ReplacementResourceId { get; private set; }
    public InstructorReplacementMode Mode { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public DateTimeOffset? AccessExpiresAtUtc { get; private set; }
}
