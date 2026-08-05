using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.Archive;

public sealed record ArchiveOrganizationSequenceCommand(
    OrganizationId OrganizationId,
    OrganizationSequenceId SequenceId,
    int ExpectedRevision) : ICommand;
