using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Branches.Events;

public sealed record BranchUpdatedDomainEvent(
    BranchId BranchId,
    OrganizationId OrganizationId,
    string PreviousName,
    string NewName,
    BranchType PreviousBranchType,
    BranchType NewBranchType,
    BranchAddress PreviousAddress,
    BranchAddress NewAddress,
    string PreviousTimeZoneId,
    string NewTimeZoneId
) : DomainEvent;
