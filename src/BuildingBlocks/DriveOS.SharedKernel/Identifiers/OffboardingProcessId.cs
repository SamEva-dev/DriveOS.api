namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct OffboardingProcessId(Guid Value)
{
    public static OffboardingProcessId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
