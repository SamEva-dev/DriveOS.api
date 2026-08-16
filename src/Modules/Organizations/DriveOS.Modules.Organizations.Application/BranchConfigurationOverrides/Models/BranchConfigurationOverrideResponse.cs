namespace DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Models;

public sealed record BranchConfigurationOverrideResponse(
    Guid Id,
    Guid OrganizationId,
    Guid BranchId,
    Guid BaseConfigurationId,
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

public sealed record BranchConfigurationOverrideListItemResponse(
    Guid Id,
    Guid BaseConfigurationId,
    int VersionNumber,
    string CountryCode,
    int Status,
    DateTimeOffset? EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    DateTimeOffset? PublishedAtUtc,
    int Revision,
    DateTimeOffset CreatedAtUtc
);
