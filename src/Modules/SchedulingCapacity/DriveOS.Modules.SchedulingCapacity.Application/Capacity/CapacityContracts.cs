namespace DriveOS.Modules.SchedulingCapacity.Application.Capacity;

public enum CapacityForecastHorizon
{
    Days7 = 1,
    Days30 = 2,
    Days90 = 3,
    Months6 = 4,
    Months12 = 5
}

public enum CapacityForecastConfidence
{
    Low = 1,
    Medium = 2,
    High = 3
}

public enum CapacityScenarioType
{
    RecruitInstructor = 1,
    AddVehicle = 2,
    ExtendOpeningHours = 3,
    UseFreelancers = 4,
    ShareWithPartner = 5,
    OpenBranch = 6
}

public sealed record CapacityForecastResponse(
    CapacityForecastHorizon Horizon,
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    DateTimeOffset GeneratedAtUtc,
    CapacityForecastConfidence Confidence,
    IReadOnlyCollection<string> Assumptions,
    CapacitySummaryResponse Summary,
    IReadOnlyCollection<CapacityDimensionResponse> ByBranch,
    IReadOnlyCollection<CapacityDimensionResponse> ByResourceType,
    IReadOnlyCollection<CapacityDimensionResponse> ByResource,
    IReadOnlyCollection<CapacityDailyResponse> Daily);

public sealed record CapacitySummaryResponse(
    decimal TheoreticalHours,
    decimal NetAvailableHours,
    decimal CommittedHours,
    decimal EstimatedDemandHours,
    decimal NetCapacityHours,
    decimal UncoveredDemandHours,
    decimal SaturationRatePercent,
    int WaitingListCount,
    decimal WaitingListHours,
    int EstimatedInstructorNeed,
    int EstimatedVehicleNeed,
    decimal? AverageSlotLeadTimeHours);

public sealed record CapacityDimensionResponse(
    string DimensionKey,
    string Label,
    decimal TheoreticalHours,
    decimal NetAvailableHours,
    decimal CommittedHours,
    decimal NetCapacityHours,
    decimal SaturationRatePercent);

public sealed record CapacityDailyResponse(
    DateOnly Date,
    decimal NetAvailableHours,
    decimal CommittedHours,
    decimal EstimatedDemandHours,
    decimal SaturationRatePercent,
    int WaitingListCount);

public sealed record CapacityScenarioRequest(
    CapacityForecastHorizon Horizon,
    CapacityScenarioType ScenarioType,
    Guid? BranchId,
    int Quantity,
    decimal AdditionalHoursPerResourcePerWeek,
    string AssumptionLabel);

public sealed record CapacityScenarioResponse(
    CapacityForecastResponse Baseline,
    CapacitySummaryResponse SimulatedSummary,
    decimal AddedNetCapacityHours,
    decimal SaturationDeltaPercent,
    IReadOnlyCollection<string> Assumptions,
    bool Applied);
