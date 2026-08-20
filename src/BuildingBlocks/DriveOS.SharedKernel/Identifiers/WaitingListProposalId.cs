namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct WaitingListProposalId(Guid Value)
{
    public static WaitingListProposalId New() => new(Guid.NewGuid());
    public static WaitingListProposalId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
