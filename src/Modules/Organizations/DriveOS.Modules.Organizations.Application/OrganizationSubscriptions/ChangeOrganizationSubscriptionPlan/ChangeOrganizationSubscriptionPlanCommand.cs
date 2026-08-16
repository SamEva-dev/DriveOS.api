using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.ChangeOrganizationSubscriptionPlan;

public sealed record ChangeOrganizationSubscriptionPlanCommand(
    OrganizationId OrganizationId,
    string PlanCode,
    IReadOnlyCollection<string> EntitlementCodes,
    IReadOnlyDictionary<string, long> Limits,
    int ExpectedVersion,
    string Reason,
    UserId ChangedByUserId
) : ICommand;
