using DriveOS.Modules.ProfessionalMarketplace.Application.Matching;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Opportunities;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Read;

public sealed class ProfessionalMatchingReadService(ProfessionalMarketplaceDbContext db):IProfessionalMatchingReadService
{
    public async Task<ProfessionalMatchResult[]> MatchAsync(
        ProfessionalOpportunityId opportunityId,
        OrganizationId organizationId,
        int limit,
        CancellationToken ct=default)
    {
        limit=Math.Clamp(limit,1,100);

        ProfessionalOpportunity? opportunity=await db.ProfessionalOpportunities.AsNoTracking()
            .SingleOrDefaultAsync(x=>x.Id==opportunityId&&x.OrganizationId==organizationId,ct);

        if(opportunity is null || opportunity.Status!=ProfessionalOpportunityStatus.Published)
            return [];

        List<ProfessionalProfile> candidates=await db.ProfessionalProfiles.AsNoTracking()
            .Where(x=>x.Status==ProfessionalProfileStatus.Active &&
                      x.MarketplaceVisibility!=MarketplaceVisibility.Private)
            .ToListAsync(ct);

        return candidates
            .Select(p=>Score(opportunity,p))
            .OrderByDescending(x=>x.Eligible)
            .ThenByDescending(x=>x.Score)
            .ThenBy(x=>x.ProfileId)
            .Take(limit)
            .ToArray();
    }

    private static ProfessionalMatchResult Score(ProfessionalOpportunity o,ProfessionalProfile p)
    {
        var blocking=new List<string>();
        var explanations=new List<string>();

        if(p.ComplianceStatus!=ProfessionalComplianceStatus.Compliant ||
           p.VerificationBadge!=MarketplaceVerificationBadge.Verified)
            blocking.Add("COMPLIANCE_NOT_VERIFIED");

        if(p.ProfessionalType!=o.ProfessionalType)
            blocking.Add("PROFESSIONAL_TYPE_MISMATCH");

        string[] categoryMatches=o.TeachingCategoryCodes.Intersect(p.TeachingCategoryCodes,StringComparer.Ordinal).ToArray();
        if(categoryMatches.Length==0)
            blocking.Add("NO_TEACHING_CATEGORY_MATCH");

        if(!o.RequiredLanguageCodes.All(x=>p.Languages.Contains(x,StringComparer.Ordinal)))
            blocking.Add("REQUIRED_LANGUAGE_MISSING");

        if(!o.RequiredSpecializationCodes.All(x=>p.SpecializationCodes.Contains(x,StringComparer.Ordinal)))
            blocking.Add("REQUIRED_SPECIALIZATION_MISSING");

        decimal? distance=DistanceKm(o,p);
        if(distance is decimal d)
        {
            if(o.RadiusKm is int orgRadius && d>orgRadius)
                blocking.Add("OUTSIDE_ORGANIZATION_RADIUS");

            ProfessionalServiceArea? primary=p.ServiceAreas.SingleOrDefault(x=>x.Primary);
            if(primary is not null &&
               primary.MobilityMode!=ProfessionalMobilityMode.Nationwide &&
               primary.RadiusKm>0 &&
               d>primary.RadiusKm)
                blocking.Add("OUTSIDE_PROFESSIONAL_RADIUS");
        }

        bool available=MatchesAvailability(o,p);
        if(!available)
            blocking.Add("COMMERCIAL_AVAILABILITY_MISMATCH");

        if(o.VehicleProvisionMode==ProfessionalVehicleProvisionMode.ProfessionalProvided &&
           !p.HasPersonalTrainingVehicle)
            blocking.Add("PROFESSIONAL_VEHICLE_REQUIRED");

        decimal categoryScore=o.TeachingCategoryCodes.Length==0?0m:
            30m*categoryMatches.Length/o.TeachingCategoryCodes.Length;

        decimal languageScore=o.RequiredLanguageCodes.Length==0?10m:
            10m*o.RequiredLanguageCodes.Count(x=>p.Languages.Contains(x,StringComparer.Ordinal))/o.RequiredLanguageCodes.Length;

        decimal specializationScore=o.RequiredSpecializationCodes.Length==0?10m:
            10m*o.RequiredSpecializationCodes.Count(x=>p.SpecializationCodes.Contains(x,StringComparer.Ordinal))/o.RequiredSpecializationCodes.Length;

        decimal distanceScore=DistanceScore(distance,o.RadiusKm,p);
        decimal availabilityScore=available?15m:0m;
        decimal vehicleScore=VehicleScore(o,p);
        decimal rateScore=RateScore(o,p);
        decimal complianceScore=p.ComplianceStatus==ProfessionalComplianceStatus.Compliant &&
                                p.VerificationBadge==MarketplaceVerificationBadge.Verified?10m:0m;

        decimal total=decimal.Round(
            categoryScore+languageScore+specializationScore+distanceScore+
            availabilityScore+vehicleScore+rateScore+complianceScore,1);

        if(categoryMatches.Length>0)
            explanations.Add($"CATEGORY_MATCH:{string.Join(',',categoryMatches)}");
        if(distance is decimal km)
            explanations.Add($"DISTANCE_KM:{decimal.Round(km,1)}");
        if(available)
            explanations.Add("COMMERCIAL_AVAILABILITY_MATCH");
        if(rateScore>0)
            explanations.Add("RATE_COMPATIBLE");
        if(complianceScore>0)
            explanations.Add("COMPLIANCE_VERIFIED");

        bool eligible=blocking.Count==0;
        if(!eligible)
            total=Math.Min(total,49.9m);

        ProfessionalRate? startingRate=p.Rates
            .Where(x=>x.EffectiveFrom<=o.StartsOn&&(x.EffectiveTo is null||o.StartsOn<=x.EffectiveTo.Value))
            .Where(x=>x.TeachingCategoryCode is null||o.TeachingCategoryCodes.Contains(x.TeachingCategoryCode,StringComparer.Ordinal))
            .OrderBy(x=>x.Amount)
            .FirstOrDefault();

        string displayName=p.TradeName??p.LegalName??p.Headline??p.Id.Value.ToString();

        return new ProfessionalMatchResult(
            p.Id.Value,displayName,p.Headline,p.ExperienceYears,p.TeachingCategoryCodes,p.Languages,p.PrimaryServiceArea,
            startingRate?.Amount,startingRate?.Currency,startingRate?.Unit.ToString(),
            total,eligible,blocking.ToArray(),
            new ProfessionalMatchBreakdown(
                decimal.Round(categoryScore,1),
                decimal.Round(languageScore,1),
                decimal.Round(specializationScore,1),
                decimal.Round(distanceScore,1),
                decimal.Round(availabilityScore,1),
                decimal.Round(vehicleScore,1),
                decimal.Round(rateScore,1),
                decimal.Round(complianceScore,1)),
            explanations.ToArray());
    }

    private static decimal DistanceScore(decimal? distance,int? searchRadius,ProfessionalProfile p)
    {
        if(distance is null)return 5m;
        if(p.ServiceAreas.Any(x=>x.MobilityMode==ProfessionalMobilityMode.Nationwide))
            return 10m;
        int reference=Math.Max(1,searchRadius??p.ServiceAreas.SingleOrDefault(x=>x.Primary)?.RadiusKm??50);
        decimal ratio=Math.Clamp(1m-distance.Value/reference,0m,1m);
        return 10m*ratio;
    }

    private static decimal VehicleScore(ProfessionalOpportunity o,ProfessionalProfile p)
    {
        return o.VehicleProvisionMode switch
        {
            ProfessionalVehicleProvisionMode.ProfessionalProvided => p.HasPersonalTrainingVehicle?5m:0m,
            ProfessionalVehicleProvisionMode.Either => p.HasPersonalTrainingVehicle?5m:4m,
            _ => 5m
        };
    }

    private static decimal RateScore(ProfessionalOpportunity o,ProfessionalProfile p)
    {
        DateOnly date=o.StartsOn;
        ProfessionalRate[] rates=p.Rates
            .Where(x=>x.EffectiveFrom<=date&&(x.EffectiveTo is null||date<=x.EffectiveTo.Value))
            .Where(x=>x.TeachingCategoryCode is null||o.TeachingCategoryCodes.Contains(x.TeachingCategoryCode,StringComparer.Ordinal))
            .Where(x=>o.BudgetUnit is null||x.Unit==o.BudgetUnit)
            .Where(x=>o.Currency is null||x.Currency==o.Currency)
            .OrderBy(x=>x.Amount)
            .ToArray();

        if(rates.Length==0)return o.BudgetMax is null?6m:0m;
        if(o.BudgetMax is null)return 10m;

        ProfessionalRate best=rates[0];
        if(best.Amount<=o.BudgetMax)return 10m;
        return best.Negotiable||o.BudgetNegotiable?5m:0m;
    }

    private static bool MatchesAvailability(ProfessionalOpportunity o,ProfessionalProfile p)
    {
        if(o.TimeWindows.Length==0)return true;

        foreach(OpportunityTimeWindow requested in o.TimeWindows)
        {
            bool recurring=p.AvailabilityPolicy.RecurringRules.Any(x=>
                x.DayOfWeek==requested.DayOfWeek &&
                x.StartTime<=requested.StartTime &&
                x.EndTime>=requested.EndTime);

            if(!recurring)return false;
        }

        return true;
    }

    private static decimal? DistanceKm(ProfessionalOpportunity o,ProfessionalProfile p)
    {
        if(o.Latitude is null||o.Longitude is null)return null;

        ProfessionalServiceArea[] areas=p.ServiceAreas
            .Where(x=>x.Latitude is not null&&x.Longitude is not null)
            .ToArray();

        if(areas.Length==0)return null;

        decimal min=decimal.MaxValue;
        foreach(ProfessionalServiceArea area in areas)
        {
            decimal km=(decimal)Haversine(
                (double)o.Latitude.Value,(double)o.Longitude.Value,
                (double)area.Latitude!.Value,(double)area.Longitude!.Value);
            if(km<min)min=km;
        }
        return min;
    }

    private static double Haversine(double lat1,double lon1,double lat2,double lon2)
    {
        const double earth=6371d;
        double dLat=R(lat2-lat1),dLon=R(lon2-lon1);
        double a=Math.Sin(dLat/2)*Math.Sin(dLat/2)+
                 Math.Cos(R(lat1))*Math.Cos(R(lat2))*
                 Math.Sin(dLon/2)*Math.Sin(dLon/2);
        return earth*2*Math.Atan2(Math.Sqrt(a),Math.Sqrt(1-a));
    }

    private static double R(double d)=>d*Math.PI/180d;
}
