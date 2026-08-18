namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct SignatureEvidenceId(Guid Value)
{
    public bool IsEmpty => Value == Guid.Empty;
    public static SignatureEvidenceId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
