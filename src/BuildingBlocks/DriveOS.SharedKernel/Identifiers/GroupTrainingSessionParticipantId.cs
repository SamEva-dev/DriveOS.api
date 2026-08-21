namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct GroupTrainingSessionParticipantId(Guid Value)
{
    public static GroupTrainingSessionParticipantId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
