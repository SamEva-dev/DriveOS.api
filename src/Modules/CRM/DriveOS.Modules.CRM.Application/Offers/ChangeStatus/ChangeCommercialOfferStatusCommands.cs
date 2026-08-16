using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Offers.ChangeStatus;

public sealed record SubmitCommercialOfferForReviewCommand(
    OrganizationId OrganizationId,
    CommercialOfferId OfferId
) : ICommand;

public sealed record ApproveCommercialOfferCommand(
    OrganizationId OrganizationId,
    CommercialOfferId OfferId
) : ICommand;
