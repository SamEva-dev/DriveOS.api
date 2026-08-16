using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Reactivate;

public sealed record ReactivateOrganizationSequenceCommand(
    OrganizationId OrganizationId,
    OrganizationSequenceId SequenceId,
    int ExpectedRevision
) : ICommand;
