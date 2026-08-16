namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct CrmActivityId(Guid Value)
{
    public static CrmActivityId New() => new(Guid.NewGuid());

    public static CrmActivityId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
