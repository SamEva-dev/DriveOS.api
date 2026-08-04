using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Subscriptions;

public sealed record SubscriptionPlanCode
{
    public const int MaximumLength = 80;

    private SubscriptionPlanCode(string value) => Value = value;

    public string Value { get; }

    public static Result<SubscriptionPlanCode> Create(string? value)
    {
        string normalized = value?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Result.Failure<SubscriptionPlanCode>(
                OrganizationSubscriptionErrors.EmptyPlanCode);
        }

        if (normalized.Length > MaximumLength)
        {
            return Result.Failure<SubscriptionPlanCode>(
                OrganizationSubscriptionErrors.PlanCodeTooLong(MaximumLength));
        }

        return Result.Success(new SubscriptionPlanCode(normalized));
    }

    public override string ToString() => Value;
}
