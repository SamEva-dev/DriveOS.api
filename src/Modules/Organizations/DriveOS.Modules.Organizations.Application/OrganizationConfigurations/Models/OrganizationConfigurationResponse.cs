namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Models;

public sealed record OrganizationConfigurationResponse(
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

public sealed record OrganizationConfigurationListItemResponse(
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
