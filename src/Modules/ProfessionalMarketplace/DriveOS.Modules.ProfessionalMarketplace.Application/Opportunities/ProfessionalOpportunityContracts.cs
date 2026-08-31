using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Opportunities;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Opportunities;

public sealed record OpportunityTimeWindowInput(DayOfWeek DayOfWeek,TimeOnly StartTime,TimeOnly EndTime,string TimeZoneId);

public sealed record CreateProfessionalOpportunityCommand(
    ProfessionalOpportunityId Id,
    OrganizationId OrganizationId,
    BranchId? BranchId,
    string Title,
    string Description,
    ProfessionalType ProfessionalType,
    string[] TeachingCategoryCodes,
    string[] RequiredLanguageCodes,
    string[] RequiredSpecializationCodes,
    string CountryCode,
    string? AreaCode,
    string? AreaDisplayName,
    decimal? Latitude,
    decimal? Longitude,
    int? RadiusKm,
    DateOnly StartsOn,
    DateOnly EndsOn,
    OpportunityTimeWindowInput[] TimeWindows,
    int? EstimatedMinutes,
    ProfessionalEngagementType EngagementType,
    ProfessionalVehicleProvisionMode VehicleProvisionMode,
    decimal? BudgetMin,
    decimal? BudgetMax,
    string? Currency,
    ProfessionalRateUnit? BudgetUnit,
    bool BudgetNegotiable,
    UserId ActorUserId) : ICommand<ProfessionalOpportunityId>;

public sealed record PublishProfessionalOpportunityCommand(ProfessionalOpportunityId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;
public sealed record PauseProfessionalOpportunityCommand(ProfessionalOpportunityId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;
public sealed record FillProfessionalOpportunityCommand(ProfessionalOpportunityId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;
public sealed record CancelProfessionalOpportunityCommand(ProfessionalOpportunityId Id,OrganizationId OrganizationId,string Reason,UserId ActorUserId):ICommand;
public sealed record GetProfessionalOpportunityQuery(ProfessionalOpportunityId Id,OrganizationId? OrganizationId):IQuery<ProfessionalOpportunityResponse>;
public sealed record ListProfessionalOpportunitiesQuery(OrganizationId OrganizationId):IQuery<IReadOnlyList<ProfessionalOpportunityResponse>>;

public sealed record ProfessionalOpportunityResponse(
    Guid Id,Guid OrganizationId,Guid? BranchId,string Status,string Title,string Description,string ProfessionalType,
    string[] TeachingCategoryCodes,string[] RequiredLanguageCodes,string[] RequiredSpecializationCodes,
    string CountryCode,string? AreaCode,string? AreaDisplayName,decimal? Latitude,decimal? Longitude,int? RadiusKm,
    DateOnly StartsOn,DateOnly EndsOn,OpportunityTimeWindowInput[] TimeWindows,int? EstimatedMinutes,string EngagementType,
    string VehicleProvisionMode,decimal? BudgetMin,decimal? BudgetMax,string? Currency,string? BudgetUnit,bool BudgetNegotiable,
    DateTimeOffset? PublishedAtUtc,DateTimeOffset? ClosedAtUtc,string? ClosureReason);
