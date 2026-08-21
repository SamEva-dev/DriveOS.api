namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct GroupTrainingSessionId(Guid Value)
{
    public static GroupTrainingSessionId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
