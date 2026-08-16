using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Endpoints.Organization.OrganizationConfigurations;

public sealed record CreateOrganizationConfigurationDraftRequest(
    int VersionNumber,
    string CountryCode,
    string PayloadJson
);

public sealed record UpdateOrganizationConfigurationDraftRequest(
    string PayloadJson,
    int ExpectedRevision
);

public sealed record PublishOrganizationConfigurationRequest(
    DateTimeOffset EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    int ExpectedRevision
);

public sealed record ArchiveOrganizationConfigurationRequest(int ExpectedRevision);

public sealed record OrganizationConfigurationResponseContract(
    Guid Id,
    Guid OrganizationId,
    int VersionNumber,
    string CountryCode,
    string PayloadJson,
    int Status,
    DateTimeOffset? EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    DateTimeOffset? PublishedAtUtc,
    Guid? PublishedByUserId,
    int Revision,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset? LastModifiedAtUtc,
    Guid? LastModifiedByUserId
);

public sealed record OrganizationConfigurationListItemResponseContract(
    Guid Id,
    int VersionNumber,
    string CountryCode,
    int Status,
    DateTimeOffset? EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    DateTimeOffset? PublishedAtUtc,
    int Revision,
    DateTimeOffset CreatedAtUtc
);

internal sealed record CreateOrganizationConfigurationDraftApiModel(
    OrganizationId OrganizationId,
    int VersionNumber,
    string CountryCode,
    string PayloadJson
);

internal sealed record UpdateOrganizationConfigurationDraftApiModel(
    OrganizationId OrganizationId,
    OrganizationConfigurationId ConfigurationId,
    string PayloadJson,
    int ExpectedRevision
);

internal sealed record PublishOrganizationConfigurationApiModel(
    OrganizationId OrganizationId,
    OrganizationConfigurationId ConfigurationId,
    DateTimeOffset EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    int ExpectedRevision
);

internal sealed record ArchiveOrganizationConfigurationApiModel(
    OrganizationId OrganizationId,
    OrganizationConfigurationId ConfigurationId,
    int ExpectedRevision
);
