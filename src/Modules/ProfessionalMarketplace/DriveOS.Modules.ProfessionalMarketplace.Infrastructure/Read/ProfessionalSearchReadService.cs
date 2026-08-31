using DriveOS.Modules.ProfessionalMarketplace.Application.Search;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Read;

public sealed class ProfessionalSearchReadService(ProfessionalMarketplaceDbContext db):IProfessionalSearchReadService
{
    public async Task<ProfessionalSearchPage> SearchAsync(SearchProfessionalsQuery q,CancellationToken ct=default)
    {
        int page=Math.Max(1,q.Page);
        int pageSize=Math.Clamp(q.PageSize,1,100);
        string? country=Token(q.CountryCode);
        string? category=Token(q.TeachingCategoryCode);
        string? language=Token(q.LanguageCode);
        string? specialization=Token(q.SpecializationCode);
        string? area=Token(q.AreaCode);
        string? currency=Token(q.Currency);

        IQueryable<ProfessionalProfile> query=db.ProfessionalProfiles.AsNoTracking()
            .Where(x=>x.Status==ProfessionalProfileStatus.Active &&
                      x.MarketplaceVisibility!=MarketplaceVisibility.Private);

        if(q.VerifiedOnly)
            query=query.Where(x=>x.ComplianceStatus==ProfessionalComplianceStatus.Compliant &&
                                 x.VerificationBadge==MarketplaceVerificationBadge.Verified);

        if(country is not null)
            query=query.Where(x=>x.BillingCountryCode==country);

        // PostgreSQL text[] predicates are safely translated before materializing richer JSON/value objects.
        if(category is not null)
            query=query.Where(x=>x.TeachingCategoryCodes.Contains(category));
        if(language is not null)
            query=query.Where(x=>x.Languages.Contains(language));
        if(specialization is not null)
            query=query.Where(x=>x.SpecializationCodes.Contains(specialization));

        List<ProfessionalProfile> candidates=await query.ToListAsync(ct);

        var filtered=candidates.Select(profile =>
        {
            double? distance=DistanceTo(profile,q.Latitude,q.Longitude);
            bool areaMatch=area is null || profile.ServiceAreas.Any(x=>x.AreaCode==area);
            bool radiusMatch=MatchesRadius(profile,distance,q.RadiusKm);
            bool available=MatchesAvailability(profile,q.AvailableOnDate,q.AvailableFrom,q.AvailableTo);
            ProfessionalRate? rate=SelectRate(profile,q.AvailableOnDate??DateOnly.FromDateTime(DateTime.UtcNow),category,q.RateUnit,currency,q.MaximumRateAmount);
            bool rateMatch=q.MaximumRateAmount is null || rate is not null;
            return new{profile,distance,areaMatch,radiusMatch,available,rate,rateMatch};
        })
        .Where(x=>x.areaMatch&&x.radiusMatch&&x.available&&x.rateMatch)
        .OrderBy(x=>x.distance??double.MaxValue)
        .ThenBy(x=>x.rate?.Amount??decimal.MaxValue)
        .ThenBy(x=>x.profile.Headline)
        .ToArray();

        int total=filtered.Length;
        var items=filtered.Skip((page-1)*pageSize).Take(pageSize)
            .Select(x=>new ProfessionalSearchResult(
                x.profile.Id.Value,
                x.profile.Headline??x.profile.TradeName??x.profile.LegalName??"Professional",
                x.profile.ProfessionalType.ToString(),
                x.profile.VerificationBadge.ToString(),
                x.profile.TeachingCategoryCodes,
                x.profile.Languages,
                x.profile.SpecializationCodes,
                x.profile.ServiceAreas.SingleOrDefault(a=>a.Primary)?.DisplayName??x.profile.PrimaryServiceArea,
                x.distance is null?null:decimal.Round((decimal)x.distance.Value,1),
                true,
                x.rate?.Amount,
                x.rate?.Currency,
                x.rate?.Unit.ToString(),
                x.rate?.Negotiable??false))
            .ToArray();

        return new ProfessionalSearchPage(page,pageSize,total,items);
    }

    private static ProfessionalRate? SelectRate(ProfessionalProfile p,DateOnly date,string? category,ProfessionalRateUnit? unit,string? currency,decimal? max)
    {
        return p.Rates
            .Where(r=>r.EffectiveFrom<=date&&(r.EffectiveTo is null||date<=r.EffectiveTo.Value))
            .Where(r=>category is null||r.TeachingCategoryCode is null||r.TeachingCategoryCode==category)
            .Where(r=>unit is null||r.Unit==unit)
            .Where(r=>currency is null||r.Currency==currency)
            .Where(r=>max is null||r.Amount<=max)
            .OrderBy(r=>r.Amount)
            .FirstOrDefault();
    }

    private static bool MatchesAvailability(ProfessionalProfile p,DateOnly? date,TimeOnly? from,TimeOnly? to)
    {
        if(date is null)return true;
        var exceptions=p.AvailabilityPolicy.Exceptions.Where(x=>x.Date==date.Value).ToArray();
        if(exceptions.Any(x=>x.Type==MarketplaceAvailabilityExceptionType.Unavailable&&x.StartTime is null))return false;
        if(exceptions.Any(x=>x.Type==MarketplaceAvailabilityExceptionType.Available&&Covers(x.StartTime,x.EndTime,from,to)))return true;

        var rules=p.AvailabilityPolicy.RecurringRules.Where(x=>x.DayOfWeek==date.Value.DayOfWeek).ToArray();
        bool recurring=rules.Any(x=>Covers(x.StartTime,x.EndTime,from,to));
        if(!recurring)return false;

        return !exceptions.Any(x=>x.Type==MarketplaceAvailabilityExceptionType.Unavailable&&Overlaps(x.StartTime,x.EndTime,from,to));
    }

    private static bool Covers(TimeOnly? start,TimeOnly? end,TimeOnly? requestedStart,TimeOnly? requestedEnd)
    {
        if(requestedStart is null&&requestedEnd is null)return true;
        if(start is null||end is null)return true;
        if(requestedStart is TimeOnly rs&&rs<start.Value)return false;
        if(requestedEnd is TimeOnly re&&re>end.Value)return false;
        return true;
    }

    private static bool Overlaps(TimeOnly? start,TimeOnly? end,TimeOnly? requestedStart,TimeOnly? requestedEnd)
    {
        if(start is null||end is null)return true;
        if(requestedStart is null&&requestedEnd is null)return true;
        TimeOnly rs=requestedStart??TimeOnly.MinValue;
        TimeOnly re=requestedEnd??TimeOnly.MaxValue;
        return start.Value<re&&rs<end.Value;
    }

    private static bool MatchesRadius(ProfessionalProfile p,double? requestedDistance,int? requestedRadius)
    {
        if(requestedDistance is null)return true;
        if(requestedRadius is int searchRadius && requestedDistance.Value>searchRadius)return false;
        if(p.ServiceAreas.Any(a=>a.MobilityMode==ProfessionalMobilityMode.Nationwide))return true;

        int professionalRadius=p.ServiceAreas
            .Where(a=>a.Primary)
            .Select(a=>a.RadiusKm)
            .DefaultIfEmpty(p.MobilityRadiusKm??0)
            .First();

        return professionalRadius<=0 || requestedDistance.Value<=professionalRadius;
    }

    private static double? DistanceTo(ProfessionalProfile p,decimal? lat,decimal? lon)
    {
        if(lat is null||lon is null)return null;
        return p.ServiceAreas.Where(x=>x.Latitude is not null&&x.Longitude is not null)
            .Select(x=>DistanceKm((double)x.Latitude!.Value,(double)x.Longitude!.Value,(double)lat.Value,(double)lon.Value))
            .Where(x=>x is not null).Select(x=>x!.Value).DefaultIfEmpty(double.MaxValue).Min();
    }

    private static double? DistanceKm(double lat1,double lon1,double? lat2,double? lon2)
    {
        if(lat2 is null||lon2 is null)return null;
        const double earth=6371d;
        double dLat=ToRadians(lat2.Value-lat1),dLon=ToRadians(lon2.Value-lon1);
        double a=Math.Sin(dLat/2)*Math.Sin(dLat/2)+Math.Cos(ToRadians(lat1))*Math.Cos(ToRadians(lat2.Value))*Math.Sin(dLon/2)*Math.Sin(dLon/2);
        return earth*2*Math.Atan2(Math.Sqrt(a),Math.Sqrt(1-a));
    }

    private static double ToRadians(double value)=>value*Math.PI/180d;
    private static string? Token(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim().ToUpperInvariant();
}
