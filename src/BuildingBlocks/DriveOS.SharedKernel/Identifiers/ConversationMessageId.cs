namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ConversationMessageId(Guid Value)
{
    public static ConversationMessageId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
