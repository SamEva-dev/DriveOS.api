using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.RegulatoryIntegrations;

public sealed record ConfigureRegulatoryIntegrationConnectionCommand(
    OrganizationId OrganizationId,
    BranchId? BranchId,
    string CountryCode,
    string ProviderCode,
    string ExternalAccountReference,
    string? SecretReference) : ICommand<RegulatoryIntegrationConnectionId>;
