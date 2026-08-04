using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Endpoints.BranchConfigurationOverrides;

public sealed record CreateBranchConfigurationOverrideDraftRequest(
    Guid BaseConfigurationId,
    int VersionNumber,
    string CountryCode,
    string PayloadJson);

public sealed record UpdateBranchConfigurationOverrideDraftRequest(
    string PayloadJson,
    int ExpectedRevision);

public sealed record PublishBranchConfigurationOverrideRequest(
    DateTimeOffset EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    int ExpectedRevision);

public sealed record ArchiveBranchConfigurationOverrideRequest(
    int ExpectedRevision);

public sealed record BranchConfigurationOverrideResponseContract(
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
    Guid? LastModifiedByUserId);

public sealed record BranchConfigurationOverrideListItemResponseContract(
    Guid Id,
    Guid BaseConfigurationId,
    int VersionNumber,
    string CountryCode,
    int Status,
    DateTimeOffset? EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    DateTimeOffset? PublishedAtUtc,
    int Revision,
    DateTimeOffset CreatedAtUtc);

internal sealed record CreateBranchConfigurationOverrideDraftApiModel(
    OrganizationId OrganizationId,
    BranchId BranchId,
    OrganizationConfigurationId BaseConfigurationId,
    int VersionNumber,
    string CountryCode,
    string PayloadJson);

internal sealed record UpdateBranchConfigurationOverrideDraftApiModel(
    OrganizationId OrganizationId,
    BranchId BranchId,
    BranchConfigurationOverrideId OverrideId,
    string PayloadJson,
    int ExpectedRevision);

internal sealed record PublishBranchConfigurationOverrideApiModel(
    OrganizationId OrganizationId,
    BranchId BranchId,
    BranchConfigurationOverrideId OverrideId,
    DateTimeOffset EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    int ExpectedRevision);

internal sealed record ArchiveBranchConfigurationOverrideApiModel(
    OrganizationId OrganizationId,
    BranchId BranchId,
    BranchConfigurationOverrideId OverrideId,
    int ExpectedRevision);
