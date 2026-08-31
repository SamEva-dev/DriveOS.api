using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Matching;

public sealed record MatchProfessionalsForOpportunityQuery(
    ProfessionalOpportunityId OpportunityId,
    OrganizationId OrganizationId,
    int Limit = 20) : IQuery<ProfessionalMatchResult[]>;

public sealed record ProfessionalMatchResult(
    Guid ProfileId,
    string DisplayName,
    string? Headline,
    int ExperienceYears,
    string[] TeachingCategoryCodes,
    string[] Languages,
    string? PrimaryServiceArea,
    decimal? StartingRateAmount,
    string? StartingRateCurrency,
    string? StartingRateUnit,
    decimal Score,
    bool Eligible,
    string[] BlockingReasons,
    ProfessionalMatchBreakdown Breakdown,
    string[] Explanations);

public sealed record ProfessionalMatchBreakdown(
    decimal CategoryScore,
    decimal LanguageScore,
    decimal SpecializationScore,
    decimal DistanceScore,
    decimal AvailabilityScore,
    decimal VehicleScore,
    decimal RateScore,
    decimal ComplianceScore);

public interface IProfessionalMatchingReadService
{
    Task<ProfessionalMatchResult[]> MatchAsync(
        ProfessionalOpportunityId opportunityId,
        OrganizationId organizationId,
        int limit,
        CancellationToken ct = default);
}
