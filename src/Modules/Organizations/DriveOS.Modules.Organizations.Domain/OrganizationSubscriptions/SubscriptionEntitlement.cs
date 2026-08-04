using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Subscriptions;

public sealed class SubscriptionEntitlement
{
    public const int CodeMaximumLength = 150;

    private SubscriptionEntitlement()
    {
    }

    private SubscriptionEntitlement(string code) => Code = code;

    public string Code { get; private set; } = string.Empty;

    internal static Result<SubscriptionEntitlement> Create(string? code)
    {
        string normalized = code?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > CodeMaximumLength)
        {
            return Result.Failure<SubscriptionEntitlement>(
                OrganizationSubscriptionErrors.InvalidEntitlementCode);
        }

        return Result.Success(new SubscriptionEntitlement(normalized));
    }
}
