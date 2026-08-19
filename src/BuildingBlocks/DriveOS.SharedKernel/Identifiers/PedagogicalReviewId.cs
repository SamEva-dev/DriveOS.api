namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct PedagogicalReviewId(Guid Value)
{
    public static PedagogicalReviewId New() => new(Guid.NewGuid());
    public static PedagogicalReviewId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
