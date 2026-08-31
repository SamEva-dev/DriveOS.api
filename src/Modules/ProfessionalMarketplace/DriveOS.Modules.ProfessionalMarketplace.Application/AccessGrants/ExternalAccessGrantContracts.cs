using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.AccessGrants;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.AccessGrants;

public sealed record CreateExternalAccessGrantCommand(
    ExternalAccessGrantId Id,
    OrganizationId OrganizationId,
    ProfessionalEngagementId EngagementId,
    ExternalAccessResourceType ResourceType,
    Guid ResourceId,
    string Permission,
    DateOnly StartDate,
    DateOnly EndDate,
    UserId ActorUserId) : ICommand<ExternalAccessGrantId>;

public sealed record RevokeExternalAccessGrantCommand(
    ExternalAccessGrantId Id,
    OrganizationId OrganizationId,
    string Reason,
    UserId ActorUserId) : ICommand;

public sealed record PrepareProfessionalEngagementAccessCommand(
    ProfessionalEngagementId EngagementId,
    OrganizationId OrganizationId,
    UserId ActorUserId) : ICommand<ExternalAccessPreparationResult>;

public sealed record ExternalAccessPreparationResult(
    bool IsPrepared,
    Guid? BaselineGrantId,
    string? ReasonCode);


public sealed record ListExternalAccessGrantsQuery(
    OrganizationId OrganizationId,
    ProfessionalEngagementId EngagementId) : IQuery<IReadOnlyList<ExternalAccessGrantReadModel>>;

public sealed record ExternalAccessGrantReadModel(
    Guid Id,
    Guid EngagementId,
    Guid ProfessionalProfileId,
    Guid OrganizationId,
    Guid? BranchId,
    ExternalAccessResourceType ResourceType,
    Guid ResourceId,
    string Permission,
    DateOnly StartDate,
    DateOnly EndDate,
    ExternalAccessGrantStatus Status,
    Guid GrantedByUserId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? RevokedAtUtc,
    Guid? RevokedByUserId,
    string? RevocationReason,
    string OriginCode);

public sealed record CheckExternalProfessionalAccessQuery(
    OrganizationId OrganizationId,
    ProfessionalProfileId ProfessionalProfileId,
    ExternalAccessResourceType ResourceType,
    Guid ResourceId,
    string Permission,
    DateOnly Date) : IQuery<bool>;
