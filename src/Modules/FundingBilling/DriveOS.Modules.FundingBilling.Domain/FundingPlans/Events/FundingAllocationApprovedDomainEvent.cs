using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.FundingPlans.Events;

public sealed record FundingAllocationApprovedDomainEvent(FundingPlanId FundingPlanId, FundingAllocationId AllocationId, decimal ApprovedAmount, string Currency, UserId ApprovedByUserId, DateTimeOffset ApprovedAtUtc) : DomainEvent;
