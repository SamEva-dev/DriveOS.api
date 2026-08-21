namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingSessionAttendanceId(Guid Value)
{
    public static TrainingSessionAttendanceId New() => new(Guid.NewGuid());
    public static TrainingSessionAttendanceId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
