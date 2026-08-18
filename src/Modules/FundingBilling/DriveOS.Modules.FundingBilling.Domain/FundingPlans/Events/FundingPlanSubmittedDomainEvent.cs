using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.FundingPlans.Events;

public sealed record FundingPlanSubmittedDomainEvent(FundingPlanId FundingPlanId, UserId SubmittedByUserId, DateTimeOffset SubmittedAtUtc) : DomainEvent;
