namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ConversationId(Guid Value)
{
    public static ConversationId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
