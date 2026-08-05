using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Suspend;

public sealed record SuspendOrganizationSequenceCommand(
    OrganizationId OrganizationId,
    OrganizationSequenceId SequenceId,
    int ExpectedRevision) : ICommand;
