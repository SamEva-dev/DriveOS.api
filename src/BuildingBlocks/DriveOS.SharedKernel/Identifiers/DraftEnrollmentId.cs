namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct DraftEnrollmentId(Guid Value)
{
    public static DraftEnrollmentId New() => new(Guid.NewGuid());

    public static DraftEnrollmentId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
