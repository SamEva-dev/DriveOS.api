using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Endpoints.Organization.OrganizationSubscriptions;

public sealed record CreateOrganizationSubscriptionRequest(
    string PlanCode,
    SubscriptionStatus Status,
    SubscriptionBillingCycle BillingCycle,
    DateTimeOffset CurrentPeriodStartsAtUtc,
    DateTimeOffset? CurrentPeriodEndsAtUtc,
    DateTimeOffset? TrialStartsAtUtc,
    DateTimeOffset? TrialEndsAtUtc,
    string? ExternalProvider,
    string? ExternalSubscriptionId);

public sealed record ChangeOrganizationSubscriptionPlanRequest(
    string PlanCode,
    IReadOnlyCollection<string> EntitlementCodes,
    IReadOnlyDictionary<string, long> Limits,
    int ExpectedVersion,
    string Reason);

public sealed record ChangeOrganizationSubscriptionStatusRequest(
    DateTimeOffset? PeriodStartsAtUtc,
    DateTimeOffset? PeriodEndsAtUtc,
    int ExpectedVersion,
    string Reason);

public sealed record CancelOrganizationSubscriptionRequest(
    DateTimeOffset EffectiveAtUtc,
    int ExpectedVersion,
    string Reason);

public sealed record SubscriptionPeriodResponseContract(
    DateTimeOffset StartsAtUtc,
    DateTimeOffset? EndsAtUtc);

public sealed record SubscriptionCancellationResponseContract(
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset EffectiveAtUtc,
    string Reason,
    Guid RequestedByUserId);

public sealed record SubscriptionEntitlementResponseContract(string Code);
public sealed record SubscriptionLimitResponseContract(string Code, long Value);

public sealed record OrganizationSubscriptionResponseContract(
    Guid Id,
    Guid OrganizationId,
    string PlanCode,
    int Status,
    int BillingCycle,
    SubscriptionPeriodResponseContract CurrentPeriod,
    SubscriptionPeriodResponseContract? TrialPeriod,
    SubscriptionCancellationResponseContract? Cancellation,
    string? ExternalProvider,
    string? ExternalSubscriptionId,
    IReadOnlyCollection<SubscriptionEntitlementResponseContract> Entitlements,
    IReadOnlyCollection<SubscriptionLimitResponseContract> Limits,
    int Version,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc);

public sealed record OrganizationEntitlementCheckResponse(
    string EntitlementCode,
    bool IsGranted);

public sealed record OrganizationLimitCheckResponseContract(
    string LimitCode,
    int Availability,
    long? Limit,
    long CurrentUsage,
    long RequestedIncrease);

internal sealed record CreateOrganizationSubscriptionApiModel(
    OrganizationId OrganizationId,
    string PlanCode,
    SubscriptionStatus Status,
    SubscriptionBillingCycle BillingCycle,
    DateTimeOffset CurrentPeriodStartsAtUtc,
    DateTimeOffset? CurrentPeriodEndsAtUtc,
    DateTimeOffset? TrialStartsAtUtc,
    DateTimeOffset? TrialEndsAtUtc,
    string? ExternalProvider,
    string? ExternalSubscriptionId);

internal sealed record ChangeOrganizationSubscriptionPlanApiModel(
    OrganizationId OrganizationId,
    string PlanCode,
    IReadOnlyCollection<string> EntitlementCodes,
    IReadOnlyDictionary<string, long> Limits,
    int ExpectedVersion,
    string Reason,
    UserId ChangedByUserId);

internal sealed record ChangeOrganizationSubscriptionStatusApiModel(
    OrganizationId OrganizationId,
    SubscriptionStatus TargetStatus,
    DateTimeOffset? PeriodStartsAtUtc,
    DateTimeOffset? PeriodEndsAtUtc,
    int ExpectedVersion,
    string Reason,
    UserId ChangedByUserId);

internal sealed record CancelOrganizationSubscriptionApiModel(
    OrganizationId OrganizationId,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset EffectiveAtUtc,
    string Reason,
    UserId RequestedByUserId,
    int ExpectedVersion);
