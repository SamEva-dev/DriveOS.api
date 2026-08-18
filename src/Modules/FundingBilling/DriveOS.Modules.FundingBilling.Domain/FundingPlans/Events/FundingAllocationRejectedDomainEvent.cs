using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.FundingPlans.Events;

public sealed record FundingAllocationRejectedDomainEvent(FundingPlanId FundingPlanId, FundingAllocationId AllocationId, string Reason, UserId RejectedByUserId, DateTimeOffset RejectedAtUtc) : DomainEvent;
