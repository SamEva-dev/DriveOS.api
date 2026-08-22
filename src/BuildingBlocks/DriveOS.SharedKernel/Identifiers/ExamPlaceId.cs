namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier used by BC-11 Exams &amp; Certification.</summary>
public readonly record struct ExamPlaceId(Guid Value)
{
    public static ExamPlaceId New() => new(Guid.NewGuid());
    public static ExamPlaceId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
