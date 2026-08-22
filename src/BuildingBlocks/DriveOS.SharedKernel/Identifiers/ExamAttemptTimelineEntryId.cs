namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ExamAttemptTimelineEntryId(Guid Value)
{
    public static ExamAttemptTimelineEntryId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
