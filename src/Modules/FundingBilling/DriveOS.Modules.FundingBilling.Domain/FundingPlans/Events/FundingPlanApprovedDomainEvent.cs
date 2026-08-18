using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.FundingPlans.Events;

public sealed record FundingPlanApprovedDomainEvent(FundingPlanId FundingPlanId, decimal TotalCost, string Currency, DateTimeOffset ApprovedAtUtc) : DomainEvent;
