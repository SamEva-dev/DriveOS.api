namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for an examination registration file.</summary>
public readonly record struct ExamRegistrationFileId(Guid Value)
{
    public static ExamRegistrationFileId New() => new(Guid.NewGuid());
    public static ExamRegistrationFileId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
