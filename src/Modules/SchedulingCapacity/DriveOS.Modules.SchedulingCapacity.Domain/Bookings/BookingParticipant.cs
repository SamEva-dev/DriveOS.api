using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public sealed class BookingParticipant
{
    private BookingParticipant() { }

    internal BookingParticipant(
        BookingParticipantId id,
        BookingId bookingId,
        BookingParticipantType participantType,
        Guid externalParticipantId)
    {
        Id = id;
        BookingId = bookingId;
        ParticipantType = participantType;
        ExternalParticipantId = externalParticipantId;
    }

    public BookingParticipantId Id { get; private set; }
    public BookingId BookingId { get; private set; }
    public BookingParticipantType ParticipantType { get; private set; }
    public Guid ExternalParticipantId { get; private set; }

    internal void ReplaceExternalParticipant(Guid externalParticipantId) => ExternalParticipantId = externalParticipantId;
}
