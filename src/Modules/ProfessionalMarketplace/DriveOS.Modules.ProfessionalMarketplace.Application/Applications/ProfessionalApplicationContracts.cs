using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Applications;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Applications;

public sealed record SubmitProfessionalApplicationCommand(
    ProfessionalApplicationId Id,
    ProfessionalOpportunityId OpportunityId,
    ProfessionalProfileId ProfessionalProfileId,
    string Message,
    decimal? ProposedRate,
    string? Currency,
    ProfessionalRateUnit? RateUnit,
    bool Negotiable,
    DateOnly? AvailableFrom,
    DateOnly? AvailableUntil,
    UserId ActorUserId):ICommand<ProfessionalApplicationId>;

public sealed record ReviewProfessionalApplicationCommand(ProfessionalApplicationId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;
public sealed record ShortlistProfessionalApplicationCommand(ProfessionalApplicationId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;
public sealed record AcceptProfessionalApplicationCommand(ProfessionalApplicationId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;
public sealed record RejectProfessionalApplicationCommand(ProfessionalApplicationId Id,OrganizationId OrganizationId,string Reason,UserId ActorUserId):ICommand;
public sealed record WithdrawProfessionalApplicationCommand(ProfessionalApplicationId Id,ProfessionalProfileId ProfileId,string? Reason,UserId ActorUserId):ICommand;


public sealed record ListProfessionalApplicationsQuery(
    OrganizationId OrganizationId,
    ProfessionalOpportunityId OpportunityId):IQuery<IReadOnlyList<ProfessionalApplicationResponse>>;

public sealed record ProfessionalApplicationResponse(
    Guid Id,Guid OpportunityId,Guid ProfessionalProfileId,string Status,string Message,
    decimal? ProposedRate,string? Currency,string? RateUnit,bool Negotiable,
    DateOnly? AvailableFrom,DateOnly? AvailableUntil,string? DecisionReason,
    DateTimeOffset SubmittedAtUtc,DateTimeOffset? DecidedAtUtc,
    string DisplayName,string? Headline,int ExperienceYears,string ComplianceStatus,
    string[] TeachingCategoryCodes,string[] Languages,string? PrimaryServiceArea);
