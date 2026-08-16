using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Conversions.Events;

public sealed record LeadConversionRequestedDomainEvent(
    LeadConversionId ConversionId,
    OrganizationId OrganizationId,
    LeadId LeadId,
    CommercialOfferId AcceptedOfferId,
    BranchId BranchId,
    UserId ResponsibleUserId
) : DomainEvent;
