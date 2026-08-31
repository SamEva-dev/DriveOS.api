using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Missions;

public sealed record MissionTimeWindowInput(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string TimeZoneId);

public sealed record CreateProfessionalMissionCommand(
    ProfessionalMissionId Id,
    OrganizationId OrganizationId,
    ProfessionalEngagementId EngagementId,
    BranchId? BranchId,
    string Title,
    string? Description,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string[] TeachingCategoryCodes,
    int? EstimatedMinutes,
    ProfessionalVehicleProvisionMode VehicleProvisionMode,
    MissionTimeWindowInput[] TimeWindows,
    UserId ActorUserId) : ICommand<ProfessionalMissionId>;

public sealed record UpdateProfessionalMissionCommand(
    ProfessionalMissionId Id,
    OrganizationId OrganizationId,
    string Title,
    string? Description,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string[] TeachingCategoryCodes,
    int? EstimatedMinutes,
    ProfessionalVehicleProvisionMode VehicleProvisionMode,
    MissionTimeWindowInput[] TimeWindows,
    UserId ActorUserId) : ICommand;

public sealed record ProposeProfessionalMissionCommand(
    ProfessionalMissionId Id, OrganizationId OrganizationId, UserId ActorUserId) : ICommand;

public sealed record AcceptProfessionalMissionCommand(
    ProfessionalMissionId Id, ProfessionalProfileId ProfileId, UserId ActorUserId) : ICommand;

public sealed record DeclineProfessionalMissionCommand(
    ProfessionalMissionId Id, ProfessionalProfileId ProfileId, string? Reason, UserId ActorUserId) : ICommand;

public sealed record ActivateProfessionalMissionCommand(
    ProfessionalMissionId Id, OrganizationId OrganizationId, UserId ActorUserId) : ICommand;

public sealed record PauseProfessionalMissionCommand(
    ProfessionalMissionId Id, OrganizationId OrganizationId, string Reason, UserId ActorUserId) : ICommand;

public sealed record ResumeProfessionalMissionCommand(
    ProfessionalMissionId Id, OrganizationId OrganizationId, UserId ActorUserId) : ICommand;

public sealed record CompleteProfessionalMissionCommand(
    ProfessionalMissionId Id, OrganizationId OrganizationId, UserId ActorUserId) : ICommand;

public sealed record CancelProfessionalMissionCommand(
    ProfessionalMissionId Id, OrganizationId OrganizationId, string Reason, UserId ActorUserId) : ICommand;


public sealed record ListProfessionalMissionsQuery(
    OrganizationId OrganizationId,
    ProfessionalEngagementId EngagementId) : IQuery<IReadOnlyList<ProfessionalMissionResponse>>;

public sealed record ListCurrentProfessionalMissionsQuery(UserId UserId)
    : IQuery<IReadOnlyList<ProfessionalMissionResponse>>;

public sealed record GetCurrentProfessionalMissionQuery(ProfessionalMissionId Id,UserId UserId)
    : IQuery<ProfessionalMissionResponse>;

public sealed record GetProfessionalMissionQuery(
    ProfessionalMissionId Id,
    OrganizationId? OrganizationId,
    ProfessionalProfileId? ProfileId) : IQuery<ProfessionalMissionResponse>;

public sealed record ProfessionalMissionResponse(
    Guid Id,
    Guid EngagementId,
    Guid OrganizationId,
    Guid ProfessionalProfileId,
    Guid? BranchId,
    string Title,
    string? Description,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string[] TeachingCategoryCodes,
    int? EstimatedMinutes,
    string VehicleProvisionMode,
    MissionTimeWindowInput[] TimeWindows,
    string Status,
    DateTimeOffset? ProposedAtUtc,
    DateTimeOffset? RespondedAtUtc,
    DateTimeOffset? ActivatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    string? StatusReason);
