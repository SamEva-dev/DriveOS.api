using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Archive;
public sealed record ArchiveOrganizationLegalProfileCommand(OrganizationId OrganizationId, int ExpectedRevision) : ICommand;
