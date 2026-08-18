namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct PaymentInstallmentId(Guid Value)
{
    public static PaymentInstallmentId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
