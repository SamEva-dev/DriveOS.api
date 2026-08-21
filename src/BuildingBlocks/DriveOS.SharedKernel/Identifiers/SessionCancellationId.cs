namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct SessionCancellationId(Guid Value)
{
    public static SessionCancellationId New() => new(Guid.NewGuid());
    public static SessionCancellationId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
