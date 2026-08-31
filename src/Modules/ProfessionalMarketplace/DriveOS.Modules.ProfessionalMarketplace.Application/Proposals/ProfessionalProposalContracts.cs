using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Proposals;

public sealed record CreateProfessionalProposalCommand(
    ProfessionalProposalId Id,
    OrganizationId OrganizationId,
    BranchId? BranchId,
    ProfessionalProfileId ProfessionalProfileId,
    ProfessionalOpportunityId? OpportunityId,
    string Subject,
    string Message,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string[] TeachingCategoryCodes,
    ProfessionalEngagementType EngagementType,
    ProfessionalVehicleProvisionMode VehicleProvisionMode,
    decimal? ProposedRate,
    string? Currency,
    ProfessionalRateUnit? RateUnit,
    bool Negotiable,
    DateTimeOffset ExpiresAtUtc,
    UserId ActorUserId):ICommand<ProfessionalProposalId>;

public sealed record AcceptProfessionalProposalCommand(ProfessionalProposalId Id,ProfessionalProfileId ProfileId,UserId ActorUserId):ICommand;
public sealed record RejectProfessionalProposalCommand(ProfessionalProposalId Id,ProfessionalProfileId ProfileId,string? Reason,UserId ActorUserId):ICommand;
public sealed record CounterProfessionalProposalCommand(ProfessionalProposalId Id,ProfessionalProfileId ProfileId,decimal ProposedRate,string Currency,ProfessionalRateUnit RateUnit,bool Negotiable,string? Message,UserId ActorUserId):ICommand;
public sealed record WithdrawProfessionalProposalCommand(ProfessionalProposalId Id,OrganizationId OrganizationId,string? Reason,UserId ActorUserId):ICommand;


public sealed record ListProfessionalProposalsQuery(OrganizationId OrganizationId,ProfessionalProfileId ProfessionalProfileId,ProfessionalOpportunityId? OpportunityId):IQuery<IReadOnlyList<ProfessionalProposalResponse>>;
public sealed record ProfessionalProposalRevisionResponse(int Revision,string Subject,string Message,DateOnly StartsOn,DateOnly EndsOn,string[] TeachingCategoryCodes,ProfessionalEngagementType EngagementType,ProfessionalVehicleProvisionMode VehicleProvisionMode,decimal? ProposedRate,string? Currency,ProfessionalRateUnit? RateUnit,bool Negotiable,DateTimeOffset ChangedAtUtc,Guid ChangedByUserId);
public sealed record ProfessionalProposalResponse(Guid Id,Guid OrganizationId,Guid? BranchId,Guid ProfessionalProfileId,Guid? OpportunityId,string Subject,string Message,DateOnly StartsOn,DateOnly EndsOn,string[] TeachingCategoryCodes,ProfessionalEngagementType EngagementType,ProfessionalVehicleProvisionMode VehicleProvisionMode,decimal? ProposedRate,string? Currency,ProfessionalRateUnit? RateUnit,bool Negotiable,DateTimeOffset ExpiresAtUtc,string Status,int Revision,string? DecisionReason,DateTimeOffset SentAtUtc,DateTimeOffset? RespondedAtUtc,IReadOnlyList<ProfessionalProposalRevisionResponse> Revisions);
