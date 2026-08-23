namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for an organization-defined Workforce job position.</summary>
public readonly record struct JobPositionId(Guid Value)
{
    public static JobPositionId New() => new(Guid.NewGuid());
    public static JobPositionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
