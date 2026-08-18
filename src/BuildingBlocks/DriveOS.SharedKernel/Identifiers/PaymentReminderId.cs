namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct PaymentReminderId(Guid Value)
{
    public bool IsEmpty => Value == Guid.Empty;
    public static PaymentReminderId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
