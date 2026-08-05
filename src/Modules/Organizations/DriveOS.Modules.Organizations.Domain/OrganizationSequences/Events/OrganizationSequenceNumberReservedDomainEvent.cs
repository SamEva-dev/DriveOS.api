using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSequences.Events;

public sealed record OrganizationSequenceNumberReservedDomainEvent(
    OrganizationSequenceId SequenceId,
    OrganizationId OrganizationId,
    BranchId? BranchId,
    string Code,
    long NumericValue,
    string FormattedValue) : DomainEvent;
