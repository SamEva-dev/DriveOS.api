namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for an official examination registration submission.</summary>
public readonly record struct ExamRegistrationSubmissionId(Guid Value)
{
    public static ExamRegistrationSubmissionId New() => new(Guid.NewGuid());
    public static ExamRegistrationSubmissionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
