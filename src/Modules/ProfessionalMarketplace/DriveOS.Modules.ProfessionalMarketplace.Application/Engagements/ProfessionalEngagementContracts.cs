using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;

public sealed record CreateProfessionalEngagementCommand(
    ProfessionalEngagementId Id,
    OrganizationId OrganizationId,
    ProfessionalCommercialOfferId CommercialOfferId,
    BranchId? BranchId,
    UserId ActorUserId):ICommand<ProfessionalEngagementId>;

public sealed record MarkEngagementPreparationCommand(
    ProfessionalEngagementId Id,
    OrganizationId OrganizationId,
    EngagementPreparationStep Step,
    bool Completed,
    UserId ActorUserId):ICommand;


public sealed record PrepareProfessionalEngagementSchedulingCommand(
    ProfessionalEngagementId Id,
    OrganizationId OrganizationId,
    UserId ActorUserId):ICommand<ProfessionalSchedulingPreparationResult>;

public sealed record ActivateProfessionalEngagementCommand(
    ProfessionalEngagementId Id,
    OrganizationId OrganizationId,
    UserId ActorUserId):ICommand;

public sealed record SuspendProfessionalEngagementCommand(
    ProfessionalEngagementId Id,
    OrganizationId OrganizationId,
    string Reason,
    UserId ActorUserId):ICommand;

public sealed record ResumeProfessionalEngagementCommand(
    ProfessionalEngagementId Id,
    OrganizationId OrganizationId,
    UserId ActorUserId):ICommand;

public sealed record CompleteProfessionalEngagementCommand(
    ProfessionalEngagementId Id,
    OrganizationId OrganizationId,
    UserId ActorUserId):ICommand<ProfessionalEngagementClosureResult>;

public sealed record TerminateProfessionalEngagementCommand(
    ProfessionalEngagementId Id,
    OrganizationId OrganizationId,
    string Reason,
    UserId ActorUserId):ICommand<ProfessionalEngagementClosureResult>;


public sealed record ProfessionalEngagementClosureResult(
    string EngagementStatus,
    int MissionsCompleted,
    int MissionsCancelled,
    int StudentAssignmentsRevoked,
    int AccessGrantsRevoked,
    bool HistoricalFinancialDataPreserved);


public sealed record ListProfessionalEngagementsQuery(
    OrganizationId OrganizationId,
    ProfessionalProfileId ProfessionalProfileId,
    ProfessionalCommercialOfferId? CommercialOfferId):IQuery<IReadOnlyList<ProfessionalEngagementView>>;

public sealed record GetProfessionalEngagementQuery(
    OrganizationId OrganizationId,
    ProfessionalEngagementId EngagementId):IQuery<ProfessionalEngagementView>;

public sealed record ProfessionalEngagementTermsView(
    DateOnly StartsOn,
    DateOnly EndsOn,
    string[] TeachingCategoryCodes,
    string EngagementType,
    string VehicleProvisionMode,
    int? EstimatedMinutes,
    decimal? RateAmount,
    string? Currency,
    string? RateUnit,
    decimal? MileageRate,
    decimal? VehicleAllowance,
    decimal? MinimumGuaranteedAmount,
    string[] ClauseCodes);

public sealed record ProfessionalEngagementView(
    Guid Id,
    Guid OrganizationId,
    Guid? BranchId,
    Guid ProfessionalProfileId,
    Guid CommercialOfferId,
    int CommercialOfferRevision,
    ProfessionalEngagementTermsView Terms,
    string Status,
    bool CompliancePrepared,
    bool ContractPrepared,
    bool AccessPrepared,
    bool SchedulingPrepared,
    bool InternalApprovalPrepared,
    bool IsOperationallyReady,
    DateTimeOffset? ActivatedAtUtc,
    DateTimeOffset? SuspendedAtUtc,
    DateTimeOffset? EndedAtUtc,
    DateTimeOffset? InitialIntegrationCompletedAtUtc,
    string? StatusReason,
    ProfessionalServiceContractSnapshot? Contract);
