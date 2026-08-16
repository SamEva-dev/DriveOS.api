using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CancelOrganizationSubscription;

public sealed record CancelOrganizationSubscriptionCommand(
    OrganizationId OrganizationId,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset EffectiveAtUtc,
    string Reason,
    UserId RequestedByUserId,
    int ExpectedVersion
) : ICommand;
