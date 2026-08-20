namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct BookingParticipantId(Guid Value)
{
    public static BookingParticipantId New() => new(Guid.NewGuid());
    public static BookingParticipantId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
