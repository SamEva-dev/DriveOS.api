namespace DriveOS.Modules.Organizations.Domain.Branches;

public sealed record BranchStatusChangeReason
{
    public const int MaximumLength = 500;

    private BranchStatusChangeReason(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static BranchStatusChangeReason Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        string normalizedValue = value.Trim();

        if (normalizedValue.Length > MaximumLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"The reason cannot exceed {MaximumLength} characters."
            );
        }

        return new BranchStatusChangeReason(normalizedValue);
    }

    public override string ToString() => Value;
}
