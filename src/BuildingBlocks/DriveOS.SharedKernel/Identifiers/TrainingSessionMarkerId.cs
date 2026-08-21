namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingSessionMarkerId(Guid Value)
{
    public static TrainingSessionMarkerId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
