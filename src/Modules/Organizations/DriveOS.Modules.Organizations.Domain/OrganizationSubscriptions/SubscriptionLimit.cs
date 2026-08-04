using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Subscriptions;

public sealed class SubscriptionLimit
{
    public const int CodeMaximumLength = 150;

    private SubscriptionLimit()
    {
    }

    private SubscriptionLimit(string code, long value)
    {
        Code = code;
        Value = value;
    }

    public string Code { get; private set; } = string.Empty;
    public long Value { get; private set; }

    internal static Result<SubscriptionLimit> Create(string? code, long value)
    {
        string normalized = code?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > CodeMaximumLength)
        {
            return Result.Failure<SubscriptionLimit>(
                OrganizationSubscriptionErrors.InvalidLimitCode);
        }

        if (value < 0)
        {
            return Result.Failure<SubscriptionLimit>(
                OrganizationSubscriptionErrors.InvalidLimitValue);
        }

        return Result.Success(new SubscriptionLimit(normalized, value));
    }
}
