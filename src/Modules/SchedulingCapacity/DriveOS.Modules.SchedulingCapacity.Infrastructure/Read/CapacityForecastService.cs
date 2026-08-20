using DriveOS.Modules.SchedulingCapacity.Application.Capacity;
using DriveOS.Modules.SchedulingCapacity.Domain.Availability;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class CapacityForecastService(SchedulingCapacityDbContext dbContext) : ICapacityForecastService
{
    public async Task<CapacityForecastResponse> ForecastAsync(
        OrganizationId organizationId,
        CapacityForecastHorizon horizon,
        BranchId? branchId,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset to = ResolveTo(now, horizon);

        CalendarResource[] resources = await dbContext.CalendarResources
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && (!branchId.HasValue || x.BranchId == branchId))
            .ToArrayAsync(cancellationToken);

        Guid[] resourceIds = resources.Select(x => x.Id.Value).ToArray();

        AvailabilityPlan[] organizationPlans = await dbContext.AvailabilityPlans
            .AsNoTracking()
            .Include(x => x.Rules)
            .Include(x => x.Exceptions)
            .Where(x => x.OrganizationId == organizationId)
            .ToArrayAsync(cancellationToken);

        HashSet<Guid> selectedResourceIds = resourceIds.ToHashSet();
        AvailabilityPlan[] plans = organizationPlans
            .Where(x => selectedResourceIds.Contains(x.CalendarResourceId.Value))
            .ToArray();

        Booking[] bookings = await dbContext.Bookings
            .AsNoTracking()
            .Include(x => x.Resources)
            .Where(x => x.OrganizationId == organizationId &&
                        (!branchId.HasValue || x.BranchId == branchId) &&
                        x.StartAtUtc < to && x.EndAtUtc > now &&
                        (x.Status == BookingStatus.Reserved || x.Status == BookingStatus.Confirmed ||
                         (x.Status == BookingStatus.Tentative && x.HoldExpiresAtUtc > now)))
            .ToArrayAsync(cancellationToken);

        WaitingListEntry[] waiting = await dbContext.WaitingListEntries
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId &&
                        (!branchId.HasValue || x.PreferredBranchId == branchId || x.PreferredBranchId == null) &&
                        x.ExpiresAtUtc > now &&
                        (x.Status == WaitingListStatus.Waiting || x.Status == WaitingListStatus.Proposed ||
                         x.Status == WaitingListStatus.TemporarilyHeld || x.Status == WaitingListStatus.Declined))
            .ToArrayAsync(cancellationToken);

        Dictionary<Guid, ResourceCapacity> capacities = resources.ToDictionary(
            x => x.Id.Value,
            x => CalculateResourceCapacity(x, plans.Where(p => p.CalendarResourceId == x.Id).ToArray(), now, to));

        foreach (Booking booking in bookings)
        {
            decimal hours = OverlapHours(booking.StartAtUtc, booking.EndAtUtc, now, to);
            if (hours <= 0m) continue;

            foreach (BookingResource bookingResource in booking.Resources)
            {
                if (capacities.TryGetValue(bookingResource.CalendarResourceId.Value, out ResourceCapacity? value))
                    value.CommittedHours += hours * bookingResource.Quantity;
            }
        }

        decimal theoretical = capacities.Values.Sum(x => x.TheoreticalHours);
        decimal netAvailable = capacities.Values.Sum(x => x.NetAvailableHours);
        decimal committed = capacities.Values.Sum(x => x.CommittedHours);
        decimal waitingHours = waiting.Sum(x => x.DurationMinutes / 60m);
        decimal demand = committed + waitingHours;
        decimal netCapacity = Math.Max(0m, netAvailable - committed);
        decimal uncoveredDemand = Math.Max(0m, demand - netAvailable);
        decimal saturation = Percent(committed, netAvailable);
        int instructorsNeeded = EstimateResourceNeed(resources, capacities, CalendarResourceType.Instructor, waitingHours, now, to);
        int vehiclesNeeded = EstimateVehicleNeed(resources, capacities, waitingHours, now, to);
        decimal? averageSlotLeadTimeHours = CalculateAverageSlotLeadTimeHours(bookings);

        CapacitySummaryResponse summary = new(
            Round(theoretical),
            Round(netAvailable),
            Round(committed),
            Round(demand),
            Round(netCapacity),
            Round(uncoveredDemand),
            Round(saturation),
            waiting.Length,
            Round(waitingHours),
            instructorsNeeded,
            vehiclesNeeded,
            averageSlotLeadTimeHours);

        IReadOnlyDictionary<Guid, string> branchLabels = BuildBranchLabels(resources);

        CapacityDimensionResponse[] byBranch = resources
            .GroupBy(x => x.BranchId?.Value)
            .Select(g => AggregateDimension(
                g.Select(x => capacities[x.Id.Value]),
                g.Key?.ToString() ?? "unassigned",
                g.Key.HasValue && branchLabels.TryGetValue(g.Key.Value, out string? label) ? label : g.Key?.ToString() ?? "Non affecté"))
            .OrderByDescending(x => x.SaturationRatePercent)
            .ToArray();

        CapacityDimensionResponse[] byType = resources
            .GroupBy(x => x.ResourceType)
            .Select(g => AggregateDimension(g.Select(x => capacities[x.Id.Value]), ((int)g.Key).ToString(), g.Key.ToString()))
            .OrderBy(x => x.DimensionKey)
            .ToArray();

        CapacityDimensionResponse[] byResource = resources
            .Where(x => x.ResourceType is CalendarResourceType.Instructor or CalendarResourceType.Vehicle or CalendarResourceType.ExamVehicle or CalendarResourceType.Room or CalendarResourceType.Simulator or CalendarResourceType.PartnerResource)
            .Select(x => AggregateDimension([capacities[x.Id.Value]], x.Id.Value.ToString(), x.DisplayName))
            .OrderByDescending(x => x.SaturationRatePercent)
            .ThenBy(x => x.Label)
            .ToArray();

        CapacityDailyResponse[] daily = BuildDaily(now, to, resources, plans, bookings, waiting);

        string[] assumptions =
        [
            "capacity.unit=resource-hours",
            "capacity.source=active-availability-plans",
            "capacity.recurring-rule-resolution=highest-priority-non-preferred-rule",
            "capacity.preferences-excluded=true",
            "capacity.tentative-active-holds-included=true",
            "demand.source=committed-bookings-plus-active-waiting-list",
            "forecast.model=deterministic-operational-baseline",
            "forecast.ai=false",
            "geography=aggregated-by-branch-only",
            "average-slot-lead-time.source=booking-created-at-to-session-start"
        ];

        CapacityForecastConfidence confidence = ResolveConfidence(resources, plans, horizon);
        return new CapacityForecastResponse(horizon, now, to, now, confidence, assumptions, summary, byBranch, byType, byResource, daily);
    }

    public async Task<CapacityScenarioResponse> SimulateAsync(
        OrganizationId organizationId,
        CapacityScenarioRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(request.Horizon) || !Enum.IsDefined(request.ScenarioType))
            throw new CapacityForecastValidationException("errors.schedulingCapacity.capacity.invalidScenario");
        if (request.Quantity is < 1 or > 1000)
            throw new CapacityForecastValidationException("errors.schedulingCapacity.capacity.invalidScenarioQuantity");
        if (request.AdditionalHoursPerResourcePerWeek is <= 0 or > 168)
            throw new CapacityForecastValidationException("errors.schedulingCapacity.capacity.invalidScenarioHours");
        if (string.IsNullOrWhiteSpace(request.AssumptionLabel) || request.AssumptionLabel.Trim().Length > 300)
            throw new CapacityForecastValidationException("errors.schedulingCapacity.capacity.scenarioAssumptionRequired");

        BranchId? branchId = request.BranchId.HasValue ? new BranchId(request.BranchId.Value) : null;
        CapacityForecastResponse baseline = await ForecastAsync(organizationId, request.Horizon, branchId, cancellationToken);
        decimal weeks = Math.Max(1m, (decimal)(baseline.ToUtc - baseline.FromUtc).TotalDays / 7m);
        decimal added = request.Quantity * request.AdditionalHoursPerResourcePerWeek * weeks;
        decimal newNetAvailable = baseline.Summary.NetAvailableHours + added;
        decimal newNetCapacity = Math.Max(0m, newNetAvailable - baseline.Summary.CommittedHours);
        decimal newUncoveredDemand = Math.Max(0m, baseline.Summary.EstimatedDemandHours - newNetAvailable);
        decimal newSaturation = Percent(baseline.Summary.CommittedHours, newNetAvailable);

        CapacitySummaryResponse simulated = baseline.Summary with
        {
            NetAvailableHours = Round(newNetAvailable),
            NetCapacityHours = Round(newNetCapacity),
            UncoveredDemandHours = Round(newUncoveredDemand),
            SaturationRatePercent = Round(newSaturation)
        };

        string[] assumptions =
        [
            request.AssumptionLabel.Trim(),
            $"scenario.type={request.ScenarioType}",
            $"scenario.quantity={request.Quantity}",
            $"scenario.additional-hours-per-resource-per-week={request.AdditionalHoursPerResourcePerWeek}",
            "scenario.capacity-unit=resource-hours",
            "scenario.applied=false"
        ];

        return new CapacityScenarioResponse(
            baseline,
            simulated,
            Round(added),
            Round(newSaturation - baseline.Summary.SaturationRatePercent),
            assumptions,
            false);
    }

    private static decimal? CalculateAverageSlotLeadTimeHours(IEnumerable<Booking> bookings)
    {
        decimal[] leadTimes = bookings
            .Where(x => x.CreatedAtUtc != default && x.StartAtUtc > x.CreatedAtUtc)
            .Select(x => (decimal)(x.StartAtUtc - x.CreatedAtUtc).TotalHours)
            .Where(x => x >= 0m)
            .ToArray();

        return leadTimes.Length == 0 ? null : Round(leadTimes.Average());
    }

    private static ResourceCapacity CalculateResourceCapacity(CalendarResource resource, AvailabilityPlan[] plans, DateTimeOffset fromUtc, DateTimeOffset toUtc)
    {
        var result = new ResourceCapacity();
        if (resource.Status == CalendarResourceStatus.Archived)
            return result;

        TimeZoneInfo timeZone;
        try { timeZone = TimeZoneInfo.FindSystemTimeZoneById(resource.TimeZoneId); }
        catch { timeZone = TimeZoneInfo.Utc; }

        DateTimeOffset localFrom = TimeZoneInfo.ConvertTime(fromUtc, timeZone);
        DateTimeOffset localTo = TimeZoneInfo.ConvertTime(toUtc, timeZone);
        DateOnly first = DateOnly.FromDateTime(localFrom.DateTime);
        DateOnly last = DateOnly.FromDateTime(localTo.AddTicks(-1).DateTime);

        for (DateOnly day = first; day <= last; day = day.AddDays(1))
        {
            AvailabilityPlan? plan = plans
                .Where(x => x.IsEffectiveOn(day))
                .OrderByDescending(x => x.EffectiveFrom)
                .FirstOrDefault();
            if (plan is null) continue;

            TimeOnly windowStart = day == first ? TimeOnly.FromDateTime(localFrom.DateTime) : TimeOnly.MinValue;
            TimeOnly windowEnd = day == last ? TimeOnly.FromDateTime(localTo.DateTime) : TimeOnly.MaxValue;
            if (windowEnd <= windowStart) continue;

            AvailabilityRule[] rules = plan.Rules
                .Where(x => x.Type != AvailabilityRuleType.Preferred && x.DayOfWeek == day.DayOfWeek)
                .Where(x => !x.BranchId.HasValue || x.BranchId == resource.BranchId)
                .ToArray();

            AvailabilityException[] exceptions = plan.Exceptions.Where(x => x.Date == day).ToArray();
            TimeOnly[] boundaries = rules.SelectMany(x => new[] { x.StartTime, x.EndTime })
                .Concat(exceptions.SelectMany(x => new[] { x.StartTime, x.EndTime }))
                .Append(windowStart)
                .Append(windowEnd)
                .Where(x => x >= windowStart && x <= windowEnd)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

            for (int index = 0; index < boundaries.Length - 1; index++)
            {
                TimeOnly segmentStart = boundaries[index];
                TimeOnly segmentEnd = boundaries[index + 1];
                if (segmentEnd <= segmentStart) continue;

                AvailabilityRule? recurring = rules
                    .Where(x => x.StartTime <= segmentStart && x.EndTime >= segmentEnd)
                    .OrderByDescending(x => x.Priority)
                    .ThenByDescending(x => x.Capacity)
                    .FirstOrDefault();

                int theoreticalCapacity = recurring?.Capacity ?? 0;
                decimal hours = (decimal)(segmentEnd - segmentStart).TotalHours;
                result.TheoreticalHours += hours * theoreticalCapacity;

                AvailabilityException? exception = exceptions
                    .Where(x => x.StartTime <= segmentStart && x.EndTime >= segmentEnd)
                    .OrderByDescending(x => x.Priority)
                    .FirstOrDefault();

                int effectiveCapacity = theoreticalCapacity;
                if (exception is not null)
                    effectiveCapacity = AvailabilityExceptionPolicy.IsUnavailable(exception.Type) ? 0 : exception.Capacity ?? 0;

                result.NetAvailableHours += hours * effectiveCapacity;
            }
        }

        if (resource.Status is CalendarResourceStatus.Unavailable or CalendarResourceStatus.Restricted)
            result.NetAvailableHours = 0m;

        result.TheoreticalHours = Math.Max(0m, result.TheoreticalHours);
        result.NetAvailableHours = Math.Max(0m, result.NetAvailableHours);
        return result;
    }

    private static CapacityDimensionResponse AggregateDimension(IEnumerable<ResourceCapacity> values, string key, string label)
    {
        ResourceCapacity[] items = values.ToArray();
        decimal theoretical = items.Sum(x => x.TheoreticalHours);
        decimal net = items.Sum(x => x.NetAvailableHours);
        decimal committed = items.Sum(x => x.CommittedHours);
        return new CapacityDimensionResponse(key, label, Round(theoretical), Round(net), Round(committed), Round(Math.Max(0m, net - committed)), Round(Percent(committed, net)));
    }

    private static CapacityDailyResponse[] BuildDaily(DateTimeOffset fromUtc, DateTimeOffset toUtc, CalendarResource[] resources, AvailabilityPlan[] plans, Booking[] bookings, WaitingListEntry[] waiting)
    {
        var list = new List<CapacityDailyResponse>();
        for (DateOnly day = DateOnly.FromDateTime(fromUtc.UtcDateTime); day <= DateOnly.FromDateTime(toUtc.AddTicks(-1).UtcDateTime); day = day.AddDays(1))
        {
            DateTimeOffset dayStart = new(day.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            DateTimeOffset dayEnd = dayStart.AddDays(1);
            DateTimeOffset sliceStart = dayStart < fromUtc ? fromUtc : dayStart;
            DateTimeOffset sliceEnd = dayEnd > toUtc ? toUtc : dayEnd;
            if (sliceEnd <= sliceStart) continue;

            decimal available = resources.Sum(r => CalculateResourceCapacity(r, plans.Where(p => p.CalendarResourceId == r.Id).ToArray(), sliceStart, sliceEnd).NetAvailableHours);
            decimal committed = bookings.Where(x => x.StartAtUtc < sliceEnd && x.EndAtUtc > sliceStart)
                .Sum(x => OverlapHours(x.StartAtUtc, x.EndAtUtc, sliceStart, sliceEnd) * Math.Max(1, x.Resources.Sum(r => r.Quantity)));
            WaitingListEntry[] dayWaiting = waiting.Where(x => x.PreferredFromUtc < sliceEnd && x.PreferredToUtc > sliceStart).ToArray();
            decimal waitingHours = dayWaiting.Sum(x => x.DurationMinutes / 60m);
            list.Add(new CapacityDailyResponse(day, Round(available), Round(committed), Round(committed + waitingHours), Round(Percent(committed, available)), dayWaiting.Length));
        }
        return list.ToArray();
    }

    private static int EstimateResourceNeed(CalendarResource[] resources, Dictionary<Guid, ResourceCapacity> capacities, CalendarResourceType type, decimal uncoveredDemandHours, DateTimeOffset from, DateTimeOffset to)
    {
        if (uncoveredDemandHours <= 0) return 0;
        ResourceCapacity[] typed = resources.Where(x => x.ResourceType == type).Select(x => capacities[x.Id.Value]).ToArray();
        return EstimateNeedFromCapacity(typed, uncoveredDemandHours, from, to);
    }

    private static int EstimateVehicleNeed(CalendarResource[] resources, Dictionary<Guid, ResourceCapacity> capacities, decimal uncoveredDemandHours, DateTimeOffset from, DateTimeOffset to)
    {
        if (uncoveredDemandHours <= 0) return 0;
        ResourceCapacity[] typed = resources
            .Where(x => x.ResourceType is CalendarResourceType.Vehicle or CalendarResourceType.ExamVehicle)
            .Select(x => capacities[x.Id.Value])
            .ToArray();
        return EstimateNeedFromCapacity(typed, uncoveredDemandHours, from, to);
    }

    private static int EstimateNeedFromCapacity(ResourceCapacity[] typed, decimal uncoveredDemandHours, DateTimeOffset from, DateTimeOffset to)
    {
        decimal totalAvailable = typed.Sum(x => x.NetAvailableHours);
        decimal totalCommitted = typed.Sum(x => x.CommittedHours);
        decimal shortage = Math.Max(0m, uncoveredDemandHours - Math.Max(0m, totalAvailable - totalCommitted));
        if (shortage <= 0m) return 0;

        decimal averageHours = typed.Length == 0 ? 0m : typed.Average(x => x.NetAvailableHours);
        if (averageHours <= 0)
        {
            decimal weeks = Math.Max(1m, (decimal)(to - from).TotalDays / 7m);
            averageHours = 35m * weeks;
        }
        return (int)Math.Ceiling(shortage / averageHours);
    }

    private static IReadOnlyDictionary<Guid, string> BuildBranchLabels(IEnumerable<CalendarResource> resources) => resources
        .Where(x => x.ResourceType == CalendarResourceType.Branch)
        .GroupBy(x => x.ExternalResourceId)
        .ToDictionary(x => x.Key, x => x.First().DisplayName);

    private static CapacityForecastConfidence ResolveConfidence(
        IReadOnlyCollection<CalendarResource> resources,
        IReadOnlyCollection<AvailabilityPlan> plans,
        CapacityForecastHorizon horizon)
    {
        if (resources.Count == 0 || plans.Count == 0)
            return CapacityForecastConfidence.Low;

        decimal coverage = (decimal)plans.Select(x => x.CalendarResourceId).Distinct().Count() / resources.Count;
        if (coverage < 0.5m)
            return CapacityForecastConfidence.Low;

        if (horizon == CapacityForecastHorizon.Days7 && coverage >= 0.8m)
            return CapacityForecastConfidence.High;

        if (horizon is CapacityForecastHorizon.Days30 or CapacityForecastHorizon.Days90)
            return CapacityForecastConfidence.Medium;

        return CapacityForecastConfidence.Low;
    }

    private static decimal OverlapHours(DateTimeOffset start, DateTimeOffset end, DateTimeOffset from, DateTimeOffset to)
    {
        DateTimeOffset effectiveStart = start > from ? start : from;
        DateTimeOffset effectiveEnd = end < to ? end : to;
        return effectiveEnd <= effectiveStart ? 0m : (decimal)(effectiveEnd - effectiveStart).TotalHours;
    }

    private static DateTimeOffset ResolveTo(DateTimeOffset from, CapacityForecastHorizon horizon) => horizon switch
    {
        CapacityForecastHorizon.Days7 => from.AddDays(7),
        CapacityForecastHorizon.Days30 => from.AddDays(30),
        CapacityForecastHorizon.Days90 => from.AddDays(90),
        CapacityForecastHorizon.Months6 => from.AddMonths(6),
        CapacityForecastHorizon.Months12 => from.AddMonths(12),
        _ => throw new CapacityForecastValidationException("errors.schedulingCapacity.capacity.invalidHorizon")
    };

    private static decimal Percent(decimal used, decimal available) => available <= 0 ? (used > 0 ? 100m : 0m) : used / available * 100m;
    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private sealed class ResourceCapacity
    {
        public decimal TheoreticalHours { get; set; }
        public decimal NetAvailableHours { get; set; }
        public decimal CommittedHours { get; set; }
    }
}

public sealed class CapacityForecastValidationException(string messageKey) : Exception(messageKey)
{
    public string MessageKey { get; } = messageKey;
}
