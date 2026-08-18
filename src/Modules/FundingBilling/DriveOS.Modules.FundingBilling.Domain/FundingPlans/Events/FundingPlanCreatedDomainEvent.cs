using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.FundingPlans.Events;

public sealed record FundingPlanCreatedDomainEvent(FundingPlanId FundingPlanId, BillingAccountId BillingAccountId, Guid ContractId, decimal TotalCost, string Currency) : DomainEvent;
