using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.FundingBilling.Domain.BillingParties;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Application.BillingParties.Manage;

public sealed record AddBillingPartyCommand(OrganizationId OrganizationId, BillingAccountId BillingAccountId, Guid? PersonId, Guid? PartyOrganizationId, BillingPartyRole Role, decimal? MaximumAmount, DateOnly EffectiveFrom, DateOnly? EffectiveTo, int Priority, bool IsPrimary, UserId ActorUserId) : ICommand<BillingPartyId>;
public sealed record EndBillingPartyCommand(OrganizationId OrganizationId, BillingPartyId BillingPartyId, string Reason, UserId ActorUserId) : ICommand;
