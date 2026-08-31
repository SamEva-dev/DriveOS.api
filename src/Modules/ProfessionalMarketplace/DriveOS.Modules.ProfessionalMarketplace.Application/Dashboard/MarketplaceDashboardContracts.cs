using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Dashboard;

public sealed record GetOrganizationMarketplaceDashboardQuery(
    OrganizationId OrganizationId,
    DateOnly From,
    DateOnly To) : IQuery<OrganizationMarketplaceDashboardResponse>;

public sealed record GetProfessionalMarketplaceDashboardQuery(
    ProfessionalProfileId ProfessionalProfileId,
    DateOnly From,
    DateOnly To) : IQuery<ProfessionalMarketplaceDashboardResponse>;

public sealed record GetCurrentProfessionalMarketplaceDashboardQuery(
    UserId UserId,
    DateOnly From,
    DateOnly To,
    ProfessionalProfileId? ExpectedProfileId = null) : IQuery<ProfessionalMarketplaceDashboardResponse>;

public sealed record MarketplaceDashboardKpis(
    int ActiveEngagements,
    int ActiveMissions,
    int PendingServiceEntries,
    int DisputedServiceEntries,
    int PendingStatements,
    int PendingInvoices,
    int ScheduledPayments,
    int FailedPayments,
    int PaidInvoices,
    int OpenReviewReports,
    int ExpiringCredentials,
    decimal? AverageValidationDelayHours,
    decimal? AveragePaymentDelayHours);


public sealed record MarketplaceDashboardAdvancedKpis(
    int InvitationsSent,
    int InvitationsAccepted,
    int InvitationsActivated,
    decimal? InvitationAcceptanceRatePercent,
    decimal? InvitationToActivationRatePercent,
    int ApplicationsDecided,
    int ApplicationsAccepted,
    decimal? ApplicationAcceptanceRatePercent,
    int CompleteProfiles,
    int ProfilesInScope,
    decimal? ProfileCompletionRatePercent,
    decimal? AverageDocumentValidationDelayHours,
    int ContractPreparedEngagements,
    decimal PlannedHours,
    decimal RealizedHours,
    int CancelledMissions,
    decimal? OccupancyRatePercent,
    int StudentsHandled,
    int ReviewedServiceEntries,
    int ServiceEntriesValidatedWithoutCorrection,
    decimal? FirstPassValidationRatePercent,
    int OverdueInvoices,
    int OpenDisputes,
    decimal? AverageHourlyCost,
    string? CostCurrency,
    int InitialIntegrationsCompleted=0,
    int ReliableRelationships=0,
    decimal? AverageInvitationToActivationDelayHours=null,
    decimal? SignedContractRatePercent=null,
    decimal? MissionCancellationRatePercent=null,
    decimal? InvoicedAmount=null,
    string? InvoicedCurrency=null,
    decimal? DisputeRatePercent=null);

public sealed record MarketplaceDashboardAlert(
    string Code,
    string Severity,
    string MessageKey,
    Guid? EntityId,
    string? EntityType,
    DateOnly? DueDate);

public sealed record OrganizationMarketplaceDashboardResponse(
    MarketplaceDashboardKpis Kpis,
    MarketplaceDashboardAlert[] Alerts,
    MarketplaceDashboardAdvancedKpis? Advanced=null);

public sealed record ProfessionalMarketplaceDashboardResponse(
    MarketplaceDashboardKpis Kpis,
    MarketplaceDashboardAlert[] Alerts,
    MarketplaceDashboardAdvancedKpis? Advanced=null);
