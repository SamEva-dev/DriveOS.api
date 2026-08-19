namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct CompetencyRecordId(Guid Value)
{
    public static CompetencyRecordId New() => new(Guid.NewGuid());
    public static CompetencyRecordId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
