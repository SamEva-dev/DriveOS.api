using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;

/// <summary>
/// Concrete operational unit performed under an active ProfessionalEngagement.
/// A mission scopes period, branch, categories, workload and intended time windows.
/// It does not itself create student assignments, lessons or Scheduling bookings.
/// </summary>
public sealed class ProfessionalMission : AggregateRoot<ProfessionalMissionId>, IAuditableEntity
{
    private ProfessionalMission() { }

    private ProfessionalMission(
        ProfessionalMissionId id,
        ProfessionalEngagement engagement,
        BranchId? branchId,
        string title,
        string? description,
        DateOnly startsOn,
        DateOnly endsOn,
        string[] teachingCategoryCodes,
        int? estimatedMinutes,
        ProfessionalVehicleProvisionMode vehicleProvisionMode,
        MissionTimeWindow[] timeWindows) : base(id)
    {
        EngagementId = engagement.Id;
        OrganizationId = engagement.OrganizationId;
        ProfessionalProfileId = engagement.ProfessionalProfileId;
        BranchId = branchId ?? engagement.BranchId;
        Title = title.Trim();
        Description = NormalizeOptional(description, 3000);
        StartsOn = startsOn;
        EndsOn = endsOn;
        TeachingCategoryCodes = NormalizeTokens(teachingCategoryCodes);
        EstimatedMinutes = estimatedMinutes;
        VehicleProvisionMode = vehicleProvisionMode;
        TimeWindows = timeWindows;
        Status = ProfessionalMissionStatus.Draft;
    }

    public ProfessionalEngagementId EngagementId { get; private set; }
    public OrganizationId OrganizationId { get; private set; }
    public ProfessionalProfileId ProfessionalProfileId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateOnly StartsOn { get; private set; }
    public DateOnly EndsOn { get; private set; }
    public string[] TeachingCategoryCodes { get; private set; } = [];
    public int? EstimatedMinutes { get; private set; }
    public ProfessionalVehicleProvisionMode VehicleProvisionMode { get; private set; }
    public MissionTimeWindow[] TimeWindows { get; private set; } = [];
    public ProfessionalMissionStatus Status { get; private set; }
    public DateTimeOffset? ProposedAtUtc { get; private set; }
    public DateTimeOffset? RespondedAtUtc { get; private set; }
    public DateTimeOffset? ActivatedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public string? StatusReason { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<ProfessionalMission> Create(
        ProfessionalMissionId id,
        ProfessionalEngagement engagement,
        BranchId? branchId,
        string title,
        string? description,
        DateOnly startsOn,
        DateOnly endsOn,
        IEnumerable<string> teachingCategoryCodes,
        int? estimatedMinutes,
        ProfessionalVehicleProvisionMode vehicleProvisionMode,
        IEnumerable<MissionTimeWindow>? timeWindows,
        DateTimeOffset nowUtc,
        UserId actorUserId)
    {
        if (id.IsEmpty || engagement.Id.IsEmpty)
            return Result.Failure<ProfessionalMission>(ProfessionalMissionErrors.InvalidIdentifier);

        if (engagement.Status != ProfessionalEngagementStatus.Active)
            return Result.Failure<ProfessionalMission>(ProfessionalMissionErrors.ActiveEngagementRequired);

        string normalizedTitle = (title ?? string.Empty).Trim();
        if (normalizedTitle.Length is < 2 or > 180)
            return Result.Failure<ProfessionalMission>(ProfessionalMissionErrors.InvalidContent);

        if (startsOn < engagement.StartsOn || endsOn > engagement.EndsOn || endsOn < startsOn)
            return Result.Failure<ProfessionalMission>(ProfessionalMissionErrors.OutsideEngagementPeriod);

        string[] categories = NormalizeTokens(teachingCategoryCodes);
        if (categories.Length == 0 ||
            categories.Any(x => !engagement.TermsSnapshot.TeachingCategoryCodes.Contains(x, StringComparer.Ordinal)))
            return Result.Failure<ProfessionalMission>(ProfessionalMissionErrors.InvalidTeachingCategories);

        if (estimatedMinutes is <= 0 or > 100000)
            return Result.Failure<ProfessionalMission>(ProfessionalMissionErrors.InvalidEstimatedWorkload);

        if (branchId.HasValue && engagement.BranchId.HasValue && branchId.Value != engagement.BranchId.Value)
            return Result.Failure<ProfessionalMission>(ProfessionalMissionErrors.BranchMismatch);

        MissionTimeWindow[] windows = (timeWindows ?? [])
            .Where(x => x is not null)
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .ToArray();

        if (windows.Any(x => x.StartTime >= x.EndTime || string.IsNullOrWhiteSpace(x.TimeZoneId)))
            return Result.Failure<ProfessionalMission>(ProfessionalMissionErrors.InvalidTimeWindows);

        foreach (IGrouping<DayOfWeek, MissionTimeWindow> group in windows.GroupBy(x => x.DayOfWeek))
        {
            MissionTimeWindow[] day = group.OrderBy(x => x.StartTime).ToArray();
            for (int i = 1; i < day.Length; i++)
                if (day[i].StartTime < day[i - 1].EndTime)
                    return Result.Failure<ProfessionalMission>(ProfessionalMissionErrors.InvalidTimeWindows);
        }

        var mission = new ProfessionalMission(
            id,
            engagement,
            branchId,
            normalizedTitle,
            description,
            startsOn,
            endsOn,
            categories,
            estimatedMinutes,
            vehicleProvisionMode,
            windows);

        mission.SetCreatedAudit(nowUtc, actorUserId);
        return Result.Success(mission);
    }

    public Result UpdateDraft(
        string title,
        string? description,
        DateOnly startsOn,
        DateOnly endsOn,
        IEnumerable<string> teachingCategoryCodes,
        int? estimatedMinutes,
        ProfessionalVehicleProvisionMode vehicleProvisionMode,
        IEnumerable<MissionTimeWindow>? timeWindows,
        DateTimeOffset nowUtc,
        UserId actorUserId)
    {
        if (Status != ProfessionalMissionStatus.Draft)
            return Result.Failure(ProfessionalMissionErrors.InvalidTransition);

        string normalizedTitle = (title ?? string.Empty).Trim();
        string[] categories = NormalizeTokens(teachingCategoryCodes);
        MissionTimeWindow[] windows = (timeWindows ?? []).OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime).ToArray();

        if (normalizedTitle.Length is < 2 or > 180 ||
            endsOn < startsOn ||
            categories.Length == 0 ||
            estimatedMinutes is <= 0 or > 100000 ||
            windows.Any(x => x.StartTime >= x.EndTime || string.IsNullOrWhiteSpace(x.TimeZoneId)))
            return Result.Failure(ProfessionalMissionErrors.InvalidContent);

        foreach (IGrouping<DayOfWeek, MissionTimeWindow> group in windows.GroupBy(x => x.DayOfWeek))
        {
            MissionTimeWindow[] day = group.OrderBy(x => x.StartTime).ToArray();
            for (int i = 1; i < day.Length; i++)
                if (day[i].StartTime < day[i - 1].EndTime)
                    return Result.Failure(ProfessionalMissionErrors.InvalidTimeWindows);
        }

        Title = normalizedTitle;
        Description = NormalizeOptional(description, 3000);
        StartsOn = startsOn;
        EndsOn = endsOn;
        TeachingCategoryCodes = categories;
        EstimatedMinutes = estimatedMinutes;
        VehicleProvisionMode = vehicleProvisionMode;
        TimeWindows = windows;
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result Propose(DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status != ProfessionalMissionStatus.Draft)
            return Result.Failure(ProfessionalMissionErrors.InvalidTransition);

        Status = ProfessionalMissionStatus.Proposed;
        ProposedAtUtc = nowUtc.ToUniversalTime();
        StatusReason = null;
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result Accept(DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status != ProfessionalMissionStatus.Proposed)
            return Result.Failure(ProfessionalMissionErrors.InvalidTransition);

        Status = ProfessionalMissionStatus.Accepted;
        RespondedAtUtc = nowUtc.ToUniversalTime();
        StatusReason = null;
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result Decline(string? reason, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status != ProfessionalMissionStatus.Proposed)
            return Result.Failure(ProfessionalMissionErrors.InvalidTransition);

        Status = ProfessionalMissionStatus.Declined;
        RespondedAtUtc = nowUtc.ToUniversalTime();
        StatusReason = NormalizeOptional(reason, 512);
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result Activate(DateOnly today, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status != ProfessionalMissionStatus.Accepted)
            return Result.Failure(ProfessionalMissionErrors.InvalidTransition);

        if (today < StartsOn || today > EndsOn)
            return Result.Failure(ProfessionalMissionErrors.OutsideMissionPeriod);

        Status = ProfessionalMissionStatus.Active;
        ActivatedAtUtc = nowUtc.ToUniversalTime();
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result Pause(string reason, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status != ProfessionalMissionStatus.Active)
            return Result.Failure(ProfessionalMissionErrors.InvalidTransition);

        string normalized = (reason ?? string.Empty).Trim();
        if (normalized.Length < 2)
            return Result.Failure(ProfessionalMissionErrors.StatusReasonRequired);

        Status = ProfessionalMissionStatus.Paused;
        StatusReason = normalized[..Math.Min(normalized.Length, 512)];
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result PauseByCompliancePolicy(string reason,DateTimeOffset nowUtc)
    {
        if(Status!=ProfessionalMissionStatus.Active)
            return Result.Failure(ProfessionalMissionErrors.InvalidTransition);

        string normalized=(reason??string.Empty).Trim();
        if(normalized.Length<2)
            return Result.Failure(ProfessionalMissionErrors.StatusReasonRequired);

        Status=ProfessionalMissionStatus.Paused;
        StatusReason=normalized[..Math.Min(normalized.Length,512)];
        SetModifiedAudit(nowUtc,null);
        return Result.Success();
    }

    public Result Resume(DateOnly today, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status != ProfessionalMissionStatus.Paused)
            return Result.Failure(ProfessionalMissionErrors.InvalidTransition);

        if (today < StartsOn || today > EndsOn)
            return Result.Failure(ProfessionalMissionErrors.OutsideMissionPeriod);

        Status = ProfessionalMissionStatus.Active;
        StatusReason = null;
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result Complete(DateOnly today, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status is not ProfessionalMissionStatus.Active and not ProfessionalMissionStatus.Paused)
            return Result.Failure(ProfessionalMissionErrors.InvalidTransition);

        if (today < EndsOn)
            return Result.Failure(ProfessionalMissionErrors.MissionNotEndedYet);

        Status = ProfessionalMissionStatus.Completed;
        CompletedAtUtc = nowUtc.ToUniversalTime();
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result Cancel(string reason, DateTimeOffset nowUtc, UserId actorUserId)
    {
        if (Status is ProfessionalMissionStatus.Completed or ProfessionalMissionStatus.Cancelled or ProfessionalMissionStatus.Declined)
            return Result.Failure(ProfessionalMissionErrors.InvalidTransition);

        string normalized = (reason ?? string.Empty).Trim();
        if (normalized.Length < 2)
            return Result.Failure(ProfessionalMissionErrors.StatusReasonRequired);

        Status = ProfessionalMissionStatus.Cancelled;
        StatusReason = normalized[..Math.Min(normalized.Length, 512)];
        CancelledAtUtc = nowUtc.ToUniversalTime();
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at, UserId? actor)
    {
        CreatedAtUtc = at.ToUniversalTime();
        CreatedByUserId = actor;
    }

    public void SetModifiedAudit(DateTimeOffset at, UserId? actor)
    {
        LastModifiedAtUtc = at.ToUniversalTime();
        LastModifiedByUserId = actor;
    }

    private static string[] NormalizeTokens(IEnumerable<string> values) =>
        values.Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

    private static string? NormalizeOptional(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        string normalized = value.Trim();
        return normalized[..Math.Min(normalized.Length, max)];
    }
}

public sealed record MissionTimeWindow(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string TimeZoneId);

public enum ProfessionalMissionStatus
{
    Draft = 1,
    Proposed = 2,
    Accepted = 3,
    Declined = 4,
    Active = 5,
    Paused = 6,
    Completed = 7,
    Cancelled = 8
}
