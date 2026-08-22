namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier used by BC-11 Exams &amp; Certification.</summary>
public readonly record struct ExamRegistrationId(Guid Value)
{
    public static ExamRegistrationId New() => new(Guid.NewGuid());
    public static ExamRegistrationId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
