using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;

/// <summary>
/// Reusable professional identity exposed by BC-13. The aggregate owns the marketplace-facing
/// professional/business profile only: authentication stays in Identity & Access, employee data
/// stays in Workforce, legal organization lifecycle stays in Organization & Tenancy, and client
/// engagements are modeled separately. Sensitive bank, insurance and credential evidence are not
/// stored here; they belong to later compliance/document capabilities.
/// </summary>
public sealed class ProfessionalProfile : AggregateRoot<ProfessionalProfileId>, IAuditableEntity
{
    private ProfessionalProfile() { }

    private ProfessionalProfile(ProfessionalProfileId id, PersonId personId, OrganizationId providerOrganizationId, UserId? userId, DateTimeOffset nowUtc) : base(id)
    {
        PersonId = personId;
        ProviderOrganizationId = providerOrganizationId;
        UserId = userId;
        Status = ProfessionalProfileStatus.Draft;
        ComplianceStatus = ProfessionalComplianceStatus.Incomplete;
        ComplianceEvaluatedAtUtc = null;
        VerificationBadge = MarketplaceVerificationBadge.None;
        MarketplaceVisibility = MarketplaceVisibility.Private;
        Languages = [];
        TeachingCategoryCodes = [];
        SpecializationCodes = [];
        TeachingCapabilities = [];
        PreferredEngagementTypes = [];
        ServiceAreas = [];
        AvailabilityPolicy = new MarketplaceAvailabilityPolicy([], [], 24, 600, 300);
        Rates = [];
        MarketplaceVisibility = MarketplaceVisibility.Private;
        VerificationBadge = MarketplaceVerificationBadge.None;
        RaiseDomainEvent(new ProfessionalProfileCreatedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, PersonId, ProviderOrganizationId));
    }

    public PersonId PersonId { get; private set; }
    public OrganizationId ProviderOrganizationId { get; private set; }
    public UserId? UserId { get; private set; }
    public ProfessionalProfileStatus Status { get; private set; }
    public ProfessionalComplianceStatus ComplianceStatus { get; private set; }
    public DateTimeOffset? ComplianceEvaluatedAtUtc { get; private set; }
    public MarketplaceVisibility MarketplaceVisibility { get; private set; }
    public MarketplaceVerificationBadge VerificationBadge { get; private set; }
    public ProfessionalComplianceEnforcementAction? ComplianceEnforcementAction { get; private set; }
    public bool NewSessionsBlocked { get; private set; }
    public bool SuspendedByCompliancePolicy { get; private set; }
    public string? ComplianceEnforcementReason { get; private set; }
    public DateOnly? ComplianceGraceUntil { get; private set; }
    public DateTimeOffset? ComplianceEnforcementUpdatedAtUtc { get; private set; }
    public bool IsDiscoverable => Status == ProfessionalProfileStatus.Active &&
                                  ComplianceStatus == ProfessionalComplianceStatus.Compliant &&
                                  MarketplaceVisibility != MarketplaceVisibility.Private;

    public ProfessionalType ProfessionalType { get; private set; } = ProfessionalType.DrivingInstructor;
    public string? LegalName { get; private set; }
    public string? TradeName { get; private set; }
    public string? LegalStatusCode { get; private set; }
    public string? RegistrationNumber { get; private set; }
    public string? TaxNumber { get; private set; }
    public string? ProfessionalEmail { get; private set; }
    public string? ProfessionalPhone { get; private set; }
    public string? BillingAddressLine1 { get; private set; }
    public string? BillingAddressLine2 { get; private set; }
    public string? BillingPostalCode { get; private set; }
    public string? BillingCity { get; private set; }
    public string? BillingCountryCode { get; private set; }

    public string? Headline { get; private set; }
    public string? Biography { get; private set; }
    public int ExperienceYears { get; private set; }
    public string[] Languages { get; private set; } = [];
    public string[] TeachingCategoryCodes { get; private set; } = [];
    public string[] SpecializationCodes { get; private set; } = [];
    public TeachingCapability[] TeachingCapabilities { get; private set; } = [];
    public string[] PreferredEngagementTypes { get; private set; } = [];

    public string? PrimaryServiceArea { get; private set; }
    public int? MobilityRadiusKm { get; private set; }
    public ProfessionalServiceArea[] ServiceAreas { get; private set; } = [];
    public MarketplaceAvailabilityPolicy AvailabilityPolicy { get; private set; } = new([], [], 24, 600, 300);
    public ProfessionalRate[] Rates { get; private set; } = [];
    public bool HasPersonalTrainingVehicle { get; private set; }
    public string? PersonalVehicleNotes { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public bool IsProfileComplete =>
        !string.IsNullOrWhiteSpace(LegalName) &&
        !string.IsNullOrWhiteSpace(LegalStatusCode) &&
        !string.IsNullOrWhiteSpace(RegistrationNumber) &&
        !string.IsNullOrWhiteSpace(ProfessionalEmail) &&
        !string.IsNullOrWhiteSpace(BillingCountryCode) &&
        !string.IsNullOrWhiteSpace(Headline) &&
        ExperienceYears >= 0 &&
        Languages.Length > 0 &&
        TeachingCategoryCodes.Length > 0 &&
        TeachingCapabilities.Length > 0 &&
        TeachingCategoryCodes.All(category => TeachingCapabilities.Any(capability => capability.CategoryCode == category)) &&
        ServiceAreas.Length > 0 &&
        ServiceAreas.Count(x => x.Primary) == 1;

    public static Result<ProfessionalProfile> Create(ProfessionalProfileId id, PersonId personId, OrganizationId providerOrganizationId, UserId? userId, DateTimeOffset nowUtc)
    {
        if (id.IsEmpty) return Result.Failure<ProfessionalProfile>(ProfessionalProfileErrors.InvalidIdentifier);
        if (personId.IsEmpty) return Result.Failure<ProfessionalProfile>(ProfessionalProfileErrors.InvalidPerson);
        if (providerOrganizationId.IsEmpty) return Result.Failure<ProfessionalProfile>(ProfessionalProfileErrors.InvalidProviderOrganization);
        return Result.Success(new ProfessionalProfile(id, personId, providerOrganizationId, userId, nowUtc));
    }

    public Result UpdateBusinessIdentity(ProfessionalType professionalType, string legalName, string? tradeName, string legalStatusCode, string registrationNumber, string? taxNumber, string professionalEmail, string? professionalPhone, string addressLine1, string? addressLine2, string postalCode, string city, string countryCode, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (!CanEdit(out Error? error)) return Result.Failure(error!);
        string country = Token(countryCode);
        if (string.IsNullOrWhiteSpace(legalName) || legalName.Trim().Length > 180 || string.IsNullOrWhiteSpace(legalStatusCode) || legalStatusCode.Trim().Length > 64 || string.IsNullOrWhiteSpace(registrationNumber) || registrationNumber.Trim().Length > 80 || string.IsNullOrWhiteSpace(professionalEmail) || professionalEmail.Trim().Length > 254 || !professionalEmail.Contains('@') || string.IsNullOrWhiteSpace(addressLine1) || addressLine1.Trim().Length > 200 || string.IsNullOrWhiteSpace(postalCode) || postalCode.Trim().Length > 32 || string.IsNullOrWhiteSpace(city) || city.Trim().Length > 120 || country.Length != 2)
            return Result.Failure(ProfessionalProfileErrors.InvalidBusinessIdentity);

        ProfessionalType = professionalType;
        LegalName = legalName.Trim();
        TradeName = Optional(tradeName, 180);
        LegalStatusCode = Token(legalStatusCode);
        RegistrationNumber = registrationNumber.Trim().ToUpperInvariant();
        TaxNumber = Optional(taxNumber, 80)?.ToUpperInvariant();
        ProfessionalEmail = professionalEmail.Trim().ToLowerInvariant();
        ProfessionalPhone = Optional(professionalPhone, 48);
        BillingAddressLine1 = addressLine1.Trim();
        BillingAddressLine2 = Optional(addressLine2, 200);
        BillingPostalCode = postalCode.Trim().ToUpperInvariant();
        BillingCity = city.Trim();
        BillingCountryCode = country;
        InvalidateVerification();
        Changed("BusinessIdentity", nowUtc, actorUserId);
        return Result.Success();
    }

    public Result UpdatePresentation(string headline, string? biography, int experienceYears, IEnumerable<string> languages, IEnumerable<string> teachingCategoryCodes, IEnumerable<string>? specializationCodes, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (!CanEdit(out Error? error)) return Result.Failure(error!);
        if (string.IsNullOrWhiteSpace(headline) || headline.Trim().Length > 160 || biography?.Trim().Length > 2000 || experienceYears is < 0 or > 80)
            return Result.Failure(ProfessionalProfileErrors.InvalidPresentation);
        string[] langs = NormalizeTokens(languages, 2, 16);
        if (langs.Length == 0) return Result.Failure(ProfessionalProfileErrors.InvalidLanguages);
        string[] categories = NormalizeTokens(teachingCategoryCodes, 1, 32);
        if (categories.Length == 0) return Result.Failure(ProfessionalProfileErrors.InvalidTeachingCategories);
        string[] specs = NormalizeTokens(specializationCodes ?? [], 1, 64);

        bool categoriesChanged = !TeachingCategoryCodes.SequenceEqual(categories, StringComparer.Ordinal);
        Headline = headline.Trim();
        Biography = Optional(biography, 2000);
        ExperienceYears = experienceYears;
        Languages = langs;
        TeachingCategoryCodes = categories;
        SpecializationCodes = specs;
        if (categoriesChanged) InvalidateVerification();
        Changed("Presentation", nowUtc, actorUserId);
        return Result.Success();
    }

    public Result ReplaceTeachingCapabilities(IEnumerable<TeachingCapability> capabilities, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (!CanEdit(out Error? error)) return Result.Failure(error!);

        TeachingCapability[] normalized = capabilities
            .Where(x => x is not null)
            .Select(x => new TeachingCapability(
                Token(x.CategoryCode),
                NormalizeTokens(x.DeliveryModeCodes ?? [], 1, 48),
                NormalizeTokens(x.AudienceCodes ?? [], 1, 48),
                NormalizeTokens(x.LanguageCodes ?? [], 2, 16),
                NormalizeTokens(x.SpecializationCodes ?? [], 1, 64)))
            .Where(x => !string.IsNullOrWhiteSpace(x.CategoryCode))
            .GroupBy(x => x.CategoryCode, StringComparer.Ordinal)
            .Select(g => g.First())
            .OrderBy(x => x.CategoryCode, StringComparer.Ordinal)
            .ToArray();

        if (normalized.Length == 0 ||
            normalized.Any(x => x.DeliveryModeCodes.Length == 0 || x.LanguageCodes.Length == 0) ||
            normalized.Any(x => !TeachingCategoryCodes.Contains(x.CategoryCode, StringComparer.Ordinal)) ||
            TeachingCategoryCodes.Any(category => normalized.All(x => x.CategoryCode != category)))
            return Result.Failure(ProfessionalProfileErrors.InvalidTeachingCapabilities);

        TeachingCapabilities = normalized;
        // A capability declaration changes the scope offered on the marketplace and must be
        // re-checked by the compliance capability before the profile can become active again.
        InvalidateVerification();
        Changed("TeachingCapabilities", nowUtc, actorUserId);
        return Result.Success();
    }

    public Result ReplaceServiceAreas(IEnumerable<ProfessionalServiceArea> areas, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (!CanEdit(out Error? error)) return Result.Failure(error!);

        ProfessionalServiceArea[] normalized = areas
            .Where(x => x is not null)
            .Select(x => new ProfessionalServiceArea(
                Token(x.AreaCode),
                Token(x.CountryCode),
                (x.DisplayName ?? string.Empty).Trim(),
                NormalizeLatitude(x.Latitude),
                NormalizeLongitude(x.Longitude),
                x.RadiusKm,
                x.Primary,
                x.MobilityMode))
            .GroupBy(x => new { x.AreaCode, x.CountryCode })
            .Select(g => g.First())
            .OrderByDescending(x => x.Primary)
            .ThenBy(x => x.CountryCode, StringComparer.Ordinal)
            .ThenBy(x => x.AreaCode, StringComparer.Ordinal)
            .ToArray();

        if (normalized.Length == 0 ||
            normalized.Count(x => x.Primary) != 1 ||
            normalized.Any(x => x.AreaCode.Length is < 1 or > 80 ||
                                x.CountryCode.Length != 2 ||
                                x.DisplayName.Length is < 2 or > 160 ||
                                x.RadiusKm is < 0 or > 500 ||
                                (x.Latitude is null) != (x.Longitude is null)))
            return Result.Failure(ProfessionalProfileErrors.InvalidServiceAreas);

        ProfessionalServiceArea primary = normalized.Single(x => x.Primary);

        if (primary.MobilityMode == ProfessionalMobilityMode.Radius && primary.RadiusKm <= 0)
            return Result.Failure(ProfessionalProfileErrors.InvalidServiceAreas);

        ServiceAreas = normalized;
        // Legacy fields are retained as a read-compatible projection during migration.
        PrimaryServiceArea = primary.DisplayName;
        MobilityRadiusKm = primary.RadiusKm == 0 ? null : primary.RadiusKm;
        Changed("ServiceAreas", nowUtc, actorUserId);
        return Result.Success();
    }

    public Result ReplaceMarketplaceAvailability(MarketplaceAvailabilityPolicy policy, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (!CanEdit(out Error? error)) return Result.Failure(error!);
        if (policy is null ||
            policy.MinimumBookingNoticeHours is < 0 or > 720 ||
            policy.MaximumDailyWorkMinutes is < 60 or > 1440 ||
            policy.MaximumConsecutiveWorkMinutes is < 30 or > 720 ||
            policy.MaximumConsecutiveWorkMinutes > policy.MaximumDailyWorkMinutes)
            return Result.Failure(ProfessionalProfileErrors.InvalidAvailabilityPolicy);

        MarketplaceAvailabilityRule[] rules = policy.RecurringRules
            .Where(x => x is not null)
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .ToArray();

        if (rules.Any(x => x.StartTime >= x.EndTime || string.IsNullOrWhiteSpace(x.TimeZoneId)))
            return Result.Failure(ProfessionalProfileErrors.InvalidAvailabilityPolicy);

        foreach (IGrouping<DayOfWeek, MarketplaceAvailabilityRule> group in rules.GroupBy(x => x.DayOfWeek))
        {
            MarketplaceAvailabilityRule[] day = group.OrderBy(x => x.StartTime).ToArray();
            for (int i = 1; i < day.Length; i++)
                if (day[i].StartTime < day[i - 1].EndTime)
                    return Result.Failure(ProfessionalProfileErrors.OverlappingAvailabilityRules);
        }

        MarketplaceAvailabilityException[] exceptions = policy.Exceptions
            .Where(x => x is not null)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.StartTime)
            .ToArray();

        if (exceptions.Any(x =>
                (x.StartTime is null) != (x.EndTime is null) ||
                (x.StartTime is TimeOnly start && x.EndTime is TimeOnly end && start >= end) ||
                x.Reason?.Trim().Length > 240))
            return Result.Failure(ProfessionalProfileErrors.InvalidAvailabilityPolicy);

        AvailabilityPolicy = new MarketplaceAvailabilityPolicy(
            rules,
            exceptions.Select(x => x with { Reason = string.IsNullOrWhiteSpace(x.Reason) ? null : x.Reason.Trim() }).ToArray(),
            policy.MinimumBookingNoticeHours,
            policy.MaximumDailyWorkMinutes,
            policy.MaximumConsecutiveWorkMinutes);

        Changed("MarketplaceAvailability", nowUtc, actorUserId);
        return Result.Success();
    }

    public Result ReplaceProfessionalRates(IEnumerable<ProfessionalRate> rates, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (!CanEdit(out Error? error)) return Result.Failure(error!);

        ProfessionalRate[] normalized = rates
            .Where(x => x is not null)
            .Select(x => new ProfessionalRate(
                Token(x.RateCode),
                x.Unit,
                decimal.Round(x.Amount, 2),
                Token(x.Currency),
                string.IsNullOrWhiteSpace(x.TeachingCategoryCode) ? null : Token(x.TeachingCategoryCode),
                x.VehicleProvisionMode,
                x.MileageRate is null ? null : decimal.Round(x.MileageRate.Value, 3),
                x.MinimumBillableQuantity is null ? null : decimal.Round(x.MinimumBillableQuantity.Value, 2),
                x.Negotiable,
                x.EffectiveFrom,
                x.EffectiveTo))
            .OrderBy(x => x.RateCode, StringComparer.Ordinal)
            .ThenBy(x => x.EffectiveFrom)
            .ToArray();

        if (normalized.Any(x =>
                x.RateCode.Length is < 1 or > 64 ||
                x.Amount < 0 ||
                x.Currency.Length != 3 ||
                (x.MileageRate is not null && x.MileageRate < 0) ||
                (x.MinimumBillableQuantity is not null && x.MinimumBillableQuantity <= 0) ||
                (x.EffectiveTo is DateOnly end && end < x.EffectiveFrom) ||
                (x.TeachingCategoryCode is not null && !TeachingCategoryCodes.Contains(x.TeachingCategoryCode, StringComparer.Ordinal))))
            return Result.Failure(ProfessionalProfileErrors.InvalidProfessionalRates);

        foreach (IGrouping<string, ProfessionalRate> group in normalized.GroupBy(x => x.RateCode, StringComparer.Ordinal))
        {
            ProfessionalRate[] ordered = group.OrderBy(x => x.EffectiveFrom).ToArray();
            for (int i = 1; i < ordered.Length; i++)
            {
                DateOnly? previousEnd = ordered[i - 1].EffectiveTo;
                if (previousEnd is null || ordered[i].EffectiveFrom <= previousEnd.Value)
                    return Result.Failure(ProfessionalProfileErrors.OverlappingProfessionalRates);
            }
        }

        Rates = normalized;
        Changed("ProfessionalRates", nowUtc, actorUserId);
        return Result.Success();
    }

    public Result UpdatePersonalVehicle(bool hasVehicle, string? notes, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (!CanEdit(out Error? error)) return Result.Failure(error!);
        if (notes?.Trim().Length > 500) return Result.Failure(ProfessionalProfileErrors.InvalidVehicleInformation);
        HasPersonalTrainingVehicle = hasVehicle;
        PersonalVehicleNotes = hasVehicle ? Optional(notes, 500) : null;
        Changed("PersonalVehicle", nowUtc, actorUserId);
        return Result.Success();
    }

    public Result ReplaceEngagementPreferences(IEnumerable<ProfessionalEngagementType> engagementTypes, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (!CanEdit(out Error? error)) return Result.Failure(error!);
        string[] values = engagementTypes.Distinct().Select(x => x.ToString()).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        if (values.Length == 0) return Result.Failure(ProfessionalProfileErrors.InvalidEngagementPreferences);
        PreferredEngagementTypes = values;
        Changed("EngagementPreferences", nowUtc, actorUserId);
        return Result.Success();
    }

    public Result CompleteProfile(DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (!CanEdit(out Error? error)) return Result.Failure(error!);
        if (!IsProfileComplete) { Status = ProfessionalProfileStatus.Incomplete; SetModifiedAudit(nowUtc, actorUserId); return Result.Failure(ProfessionalProfileErrors.ProfileIncomplete); }
        Status = ProfessionalProfileStatus.PendingVerification;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new ProfessionalProfileCompletedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, PersonId, ProviderOrganizationId, actorUserId));
        return Result.Success();
    }

    public Result MarkCompliance(ProfessionalComplianceStatus status, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status == ProfessionalProfileStatus.Archived) return Result.Failure(ProfessionalProfileErrors.Archived);
        ComplianceStatus = status;
        ComplianceEvaluatedAtUtc = nowUtc.ToUniversalTime();
        VerificationBadge = status switch
        {
            ProfessionalComplianceStatus.Compliant => MarketplaceVerificationBadge.Verified,
            ProfessionalComplianceStatus.PendingReview => MarketplaceVerificationBadge.Pending,
            ProfessionalComplianceStatus.PartiallyCompliant => MarketplaceVerificationBadge.Pending,
            ProfessionalComplianceStatus.Suspended => MarketplaceVerificationBadge.Restricted,
            _ => MarketplaceVerificationBadge.None
        };
        if (status == ProfessionalComplianceStatus.Compliant && IsProfileComplete && Status is ProfessionalProfileStatus.Draft or ProfessionalProfileStatus.Incomplete or ProfessionalProfileStatus.PendingVerification)
            Status = ProfessionalProfileStatus.PendingVerification;
        if (status != ProfessionalComplianceStatus.Compliant)
        {
            MarketplaceVisibility = MarketplaceVisibility.Private;
            if (Status == ProfessionalProfileStatus.Active)
                Status = ProfessionalProfileStatus.Suspended;
        }
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result MarkComplianceAutomated(ProfessionalComplianceStatus status, DateTimeOffset nowUtc)
    {
        if (Status == ProfessionalProfileStatus.Archived) return Result.Failure(ProfessionalProfileErrors.Archived);

        ComplianceStatus = status;
        ComplianceEvaluatedAtUtc = nowUtc.ToUniversalTime();
        VerificationBadge = status switch
        {
            ProfessionalComplianceStatus.Compliant => MarketplaceVerificationBadge.Verified,
            ProfessionalComplianceStatus.PendingReview => MarketplaceVerificationBadge.Pending,
            ProfessionalComplianceStatus.PartiallyCompliant => MarketplaceVerificationBadge.Pending,
            ProfessionalComplianceStatus.Suspended => MarketplaceVerificationBadge.Restricted,
            _ => MarketplaceVerificationBadge.None
        };

        if (status != ProfessionalComplianceStatus.Compliant)
            MarketplaceVisibility = MarketplaceVisibility.Private;

        SetModifiedAudit(nowUtc, null);
        return Result.Success();
    }

    public Result ApplyComplianceEnforcement(
        ProfessionalComplianceEnforcementAction action,
        string reason,
        DateOnly? graceUntil,
        DateTimeOffset nowUtc)
    {
        if(Status==ProfessionalProfileStatus.Archived)
            return Result.Failure(ProfessionalProfileErrors.Archived);

        string normalized=(reason??string.Empty).Trim();
        if(normalized.Length is <2 or >1000)
            return Result.Failure(Error.Validation(
                "ProfessionalMarketplace.Profile.InvalidComplianceEnforcement",
                "errors.professionalMarketplace.profile.invalidComplianceEnforcement"));

        ComplianceEnforcementAction=action;
        ComplianceEnforcementReason=normalized;
        ComplianceGraceUntil=graceUntil;
        ComplianceEnforcementUpdatedAtUtc=nowUtc.ToUniversalTime();
        NewSessionsBlocked=action is
            ProfessionalComplianceEnforcementAction.BlockNewSessions or
            ProfessionalComplianceEnforcementAction.PauseMissions or
            ProfessionalComplianceEnforcementAction.SuspendProfessional;

        if(action==ProfessionalComplianceEnforcementAction.SuspendProfessional)
        {
            MarketplaceVisibility=MarketplaceVisibility.Private;
            if(Status==ProfessionalProfileStatus.Active)
            {
                Status=ProfessionalProfileStatus.Suspended;
                SuspendedByCompliancePolicy=true;
            }
        }

        SetModifiedAudit(nowUtc,null);
        return Result.Success();
    }

    public Result ClearComplianceEnforcement(DateTimeOffset nowUtc)
    {
        if(Status==ProfessionalProfileStatus.Archived)
            return Result.Failure(ProfessionalProfileErrors.Archived);

        ComplianceEnforcementAction=null;
        ComplianceEnforcementReason=null;
        ComplianceGraceUntil=null;
        ComplianceEnforcementUpdatedAtUtc=nowUtc.ToUniversalTime();
        NewSessionsBlocked=false;

        if(SuspendedByCompliancePolicy)
        {
            Status=ProfessionalProfileStatus.Active;
            SuspendedByCompliancePolicy=false;
        }

        SetModifiedAudit(nowUtc,null);
        return Result.Success();
    }

    public Result ChangeMarketplaceVisibility(MarketplaceVisibility visibility, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status == ProfessionalProfileStatus.Archived) return Result.Failure(ProfessionalProfileErrors.Archived);
        if (visibility != MarketplaceVisibility.Private &&
            (ComplianceStatus != ProfessionalComplianceStatus.Compliant || VerificationBadge != MarketplaceVerificationBadge.Verified))
            return Result.Failure(ProfessionalProfileErrors.VerifiedProfileRequiredForVisibility);
        MarketplaceVisibility = visibility;
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result Activate(DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status == ProfessionalProfileStatus.Archived) return Result.Failure(ProfessionalProfileErrors.Archived);
        if (Status == ProfessionalProfileStatus.Active) return Result.Failure(ProfessionalProfileErrors.AlreadyActive);
        if (!IsProfileComplete) return Result.Failure(ProfessionalProfileErrors.ProfileIncomplete);
        if (ComplianceStatus != ProfessionalComplianceStatus.Compliant) return Result.Failure(ProfessionalProfileErrors.ComplianceRequired);
        Status = ProfessionalProfileStatus.Active;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new ProfessionalProfileActivatedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, PersonId, ProviderOrganizationId, actorUserId));
        return Result.Success();
    }

    public Result Suspend(string reason, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status == ProfessionalProfileStatus.Archived) return Result.Failure(ProfessionalProfileErrors.Archived);
        if (string.IsNullOrWhiteSpace(reason)) return Result.Failure(Error.Validation("ProfessionalMarketplace.Profile.SuspendReasonRequired", "errors.professionalMarketplace.profile.suspendReasonRequired"));
        Status = ProfessionalProfileStatus.Suspended;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new ProfessionalProfileSuspendedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, ProviderOrganizationId, reason.Trim(), actorUserId));
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId) { if (CreatedAtUtc != default) return; CreatedAtUtc = createdAtUtc.ToUniversalTime(); CreatedByUserId = createdByUserId; }
    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId) { LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime(); LastModifiedByUserId = modifiedByUserId; }

    private bool CanEdit(out Error? error) { error = Status == ProfessionalProfileStatus.Archived ? ProfessionalProfileErrors.Archived : null; return error is null; }
    private void InvalidateVerification() { ComplianceStatus = ProfessionalComplianceStatus.Incomplete; if (Status != ProfessionalProfileStatus.Suspended) Status = ProfessionalProfileStatus.Incomplete; }
    private void Changed(string type, DateTimeOffset nowUtc, UserId actor) { SetModifiedAudit(nowUtc, actor); RaiseDomainEvent(new ProfessionalProfileUpdatedDomainEvent(Guid.NewGuid(), nowUtc.ToUniversalTime(), Id, ProviderOrganizationId, type, actor)); }
    private static decimal? NormalizeLatitude(decimal? value) =>
        value is >= -90m and <= 90m ? decimal.Round(value.Value, 3) : value is null ? null : 999m;

    private static decimal? NormalizeLongitude(decimal? value) =>
        value is >= -180m and <= 180m ? decimal.Round(value.Value, 3) : value is null ? null : 999m;

    private static string Token(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();
    private static string? Optional(string? value, int max) { if (string.IsNullOrWhiteSpace(value)) return null; string x = value.Trim(); return x.Length <= max ? x : x[..max]; }
    private static string[] NormalizeTokens(IEnumerable<string> values, int min, int max) => values.Where(x => !string.IsNullOrWhiteSpace(x)).Select(Token).Where(x => x.Length >= min && x.Length <= max).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
}
