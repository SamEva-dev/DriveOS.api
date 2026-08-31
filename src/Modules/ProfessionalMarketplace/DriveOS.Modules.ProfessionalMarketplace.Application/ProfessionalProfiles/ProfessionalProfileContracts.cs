using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ProfessionalMarketplace.Application.ProfessionalProfiles;
public sealed record CreateProfessionalProfileCommand(ProfessionalProfileId Id,PersonId PersonId,OrganizationId ProviderOrganizationId,UserId? UserId,UserId ActorUserId):ICommand<ProfessionalProfileId>;
public sealed record UpdateProfessionalBusinessIdentityCommand(ProfessionalProfileId Id,ProfessionalType ProfessionalType,string LegalName,string? TradeName,string LegalStatusCode,string RegistrationNumber,string? TaxNumber,string ProfessionalEmail,string? ProfessionalPhone,string AddressLine1,string? AddressLine2,string PostalCode,string City,string CountryCode,UserId ActorUserId):ICommand;
public sealed record UpdateProfessionalPresentationCommand(ProfessionalProfileId Id,string Headline,string? Biography,int ExperienceYears,string[] Languages,string[] TeachingCategoryCodes,string[]? SpecializationCodes,UserId ActorUserId):ICommand;
public sealed record ProfessionalServiceAreaInput(string AreaCode,string CountryCode,string DisplayName,decimal? Latitude,decimal? Longitude,int RadiusKm,bool Primary,ProfessionalMobilityMode MobilityMode);
public sealed record ReplaceProfessionalServiceAreasCommand(ProfessionalProfileId Id,ProfessionalServiceAreaInput[] Areas,UserId ActorUserId):ICommand;
public sealed record MarketplaceAvailabilityRuleInput(DayOfWeek DayOfWeek,TimeOnly StartTime,TimeOnly EndTime,string TimeZoneId);
public sealed record MarketplaceAvailabilityExceptionInput(DateOnly Date,TimeOnly? StartTime,TimeOnly? EndTime,MarketplaceAvailabilityExceptionType Type,string? Reason);
public sealed record ReplaceMarketplaceAvailabilityCommand(
    ProfessionalProfileId Id,
    MarketplaceAvailabilityRuleInput[] RecurringRules,
    MarketplaceAvailabilityExceptionInput[] Exceptions,
    int MinimumBookingNoticeHours,
    int MaximumDailyWorkMinutes,
    int MaximumConsecutiveWorkMinutes,
    UserId ActorUserId):ICommand;

public sealed record ProfessionalRateInput(
    string RateCode,
    ProfessionalRateUnit Unit,
    decimal Amount,
    string Currency,
    string? TeachingCategoryCode,
    ProfessionalVehicleProvisionMode VehicleProvisionMode,
    decimal? MileageRate,
    decimal? MinimumBillableQuantity,
    bool Negotiable,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo);
public sealed record ReplaceProfessionalRatesCommand(ProfessionalProfileId Id,ProfessionalRateInput[] Rates,UserId ActorUserId):ICommand;

public sealed record UpdateProfessionalVehicleCommand(ProfessionalProfileId Id,bool HasPersonalTrainingVehicle,string? Notes,UserId ActorUserId):ICommand;
public sealed record UpdateProfessionalEngagementPreferencesCommand(ProfessionalProfileId Id,ProfessionalEngagementType[] EngagementTypes,UserId ActorUserId):ICommand;
public sealed record CompleteProfessionalProfileCommand(ProfessionalProfileId Id,UserId ActorUserId):ICommand;
public sealed record GetProfessionalProfileQuery(ProfessionalProfileId Id):IQuery<ProfessionalProfileResponse>;
public sealed record GetCurrentProfessionalProfileQuery(UserId UserId):IQuery<ProfessionalProfileResponse>;
public sealed record ProfessionalProfileResponse(Guid Id,Guid PersonId,Guid ProviderOrganizationId,Guid? UserId,string Status,string ComplianceStatus,string ProfessionalType,string? LegalName,string? TradeName,string? LegalStatusCode,string? RegistrationNumber,string? TaxNumber,string? ProfessionalEmail,string? ProfessionalPhone,string? AddressLine1,string? AddressLine2,string? PostalCode,string? City,string? CountryCode,string? Headline,string? Biography,int ExperienceYears,string[] Languages,string[] TeachingCategoryCodes,string[] SpecializationCodes,TeachingCapabilityResponse[] TeachingCapabilities,string[] PreferredEngagementTypes,string? PrimaryServiceArea,int? MobilityRadiusKm,ProfessionalServiceAreaInput[] ServiceAreas,MarketplaceAvailabilityRuleInput[] AvailabilityRules,MarketplaceAvailabilityExceptionInput[] AvailabilityExceptions,int MinimumBookingNoticeHours,int MaximumDailyWorkMinutes,int MaximumConsecutiveWorkMinutes,ProfessionalRateInput[] Rates,bool HasPersonalTrainingVehicle,string? PersonalVehicleNotes,bool IsProfileComplete);

public sealed record TeachingCapabilityInput(
    string CategoryCode,
    string[] DeliveryModeCodes,
    string[] AudienceCodes,
    string[] LanguageCodes,
    string[] SpecializationCodes);

public sealed record ReplaceTeachingCapabilitiesCommand(
    ProfessionalProfileId ProfileId,
    TeachingCapabilityInput[] Capabilities,
    UserId ActorUserId) : ICommand;


public sealed record TeachingCapabilityResponse(string CategoryCode,string[] DeliveryModeCodes,string[] AudienceCodes,string[] LanguageCodes,string[] SpecializationCodes);
