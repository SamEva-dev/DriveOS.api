namespace DriveOS.Modules.Organizations.Domain.Organizations;

public sealed record OrganizationStatusChangeReason
{
    public const int MaximumLength = 500;

    private OrganizationStatusChangeReason(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static OrganizationStatusChangeReason Create(
        string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        string normalizedValue = value.Trim();

        if (normalizedValue.Length > MaximumLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"The reason cannot exceed {MaximumLength} characters.");
        }

        return new OrganizationStatusChangeReason(
            normalizedValue);
    }

    public override string ToString() => Value;
}