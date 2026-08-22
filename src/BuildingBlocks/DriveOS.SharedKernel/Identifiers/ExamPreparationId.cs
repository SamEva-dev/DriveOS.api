namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for the preparation of an exam registration.</summary>
public readonly record struct ExamPreparationId(Guid Value)
{
    public static ExamPreparationId New() => new(Guid.NewGuid());
    public static ExamPreparationId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
