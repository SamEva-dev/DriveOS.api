using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Reserve;

public sealed record ReserveOrganizationSequenceNumberCommand(
    OrganizationId OrganizationId,
    BranchId? BranchId,
    string Code)
    : ICommand<string>;
