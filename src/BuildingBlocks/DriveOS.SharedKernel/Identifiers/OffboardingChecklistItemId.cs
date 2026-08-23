namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct OffboardingChecklistItemId(Guid Value)
{
    public static OffboardingChecklistItemId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
