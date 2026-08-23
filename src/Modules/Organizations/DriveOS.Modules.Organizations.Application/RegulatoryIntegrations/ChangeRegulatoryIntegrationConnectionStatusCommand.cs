using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.RegulatoryIntegrations;

public sealed record ChangeRegulatoryIntegrationConnectionStatusCommand(
    OrganizationId OrganizationId,
    RegulatoryIntegrationConnectionId ConnectionId,
    RegulatoryIntegrationConnectionStatus Status,
    int ExpectedRevision) : ICommand;
