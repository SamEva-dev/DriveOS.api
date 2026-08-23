using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.RegulatoryIntegrations;

public sealed record UpdateRegulatoryIntegrationConnectionCommand(
    OrganizationId OrganizationId,
    RegulatoryIntegrationConnectionId ConnectionId,
    string ExternalAccountReference,
    string? SecretReference,
    int ExpectedRevision) : ICommand;
