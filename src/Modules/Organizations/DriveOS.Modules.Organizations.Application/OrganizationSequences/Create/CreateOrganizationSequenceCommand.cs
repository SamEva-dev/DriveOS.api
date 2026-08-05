using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Create;

public sealed record CreateOrganizationSequenceCommand(
    OrganizationId OrganizationId,
    BranchId? BranchId,
    OrganizationSequenceScope Scope,
    string Code,
    string Pattern,
    int Padding,
    long InitialValue,
    OrganizationSequenceResetPolicy ResetPolicy)
    : ICommand<OrganizationSequenceId>;
