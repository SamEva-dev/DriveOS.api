using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;

public sealed record WaitingListPriorityInput(
    DateTimeOffset? ExamAtUtc,
    bool HasNoFutureSession,
    int InterruptionDays,
    int PedagogicalUrgencyLevel,
    bool SchoolCancellation,
    bool LimitedAvailability,
    bool RegulatoryPriority,
    bool CommercialPriority,
    int ManualAdjustment,
    string? ManualAdjustmentReason);

public sealed record WaitingListPriorityResult(int BaseScore, string Explanation);

public static class WaitingListPriorityPolicy
{
    public static Result<WaitingListPriorityResult> Calculate(WaitingListPriorityInput input, DateTimeOffset nowUtc)
    {
        if (input.InterruptionDays < 0 || input.PedagogicalUrgencyLevel is < 0 or > 3 || input.ManualAdjustment is < -20 or > 20)
            return Result.Failure<WaitingListPriorityResult>(WaitingListErrors.InvalidPriority);
        if (input.ManualAdjustment != 0 && string.IsNullOrWhiteSpace(input.ManualAdjustmentReason))
            return Result.Failure<WaitingListPriorityResult>(WaitingListErrors.InvalidPriority);

        int score = 0;
        List<string> factors = [];
        if (input.ExamAtUtc.HasValue)
        {
            double days = (input.ExamAtUtc.Value.ToUniversalTime() - nowUtc).TotalDays;
            int points = days <= 7 ? 30 : days <= 14 ? 20 : days <= 30 ? 10 : 0;
            if (points > 0) { score += points; factors.Add($"examSoon:+{points}"); }
        }
        if (input.HasNoFutureSession) { score += 15; factors.Add("noFutureSession:+15"); }
        int interruption = input.InterruptionDays >= 21 ? 15 : input.InterruptionDays >= 14 ? 10 : input.InterruptionDays >= 7 ? 5 : 0;
        if (interruption > 0) { score += interruption; factors.Add($"interruption:+{interruption}"); }
        int urgency = input.PedagogicalUrgencyLevel * 10;
        if (urgency > 0) { score += urgency; factors.Add($"pedagogicalUrgency:+{urgency}"); }
        if (input.SchoolCancellation) { score += 20; factors.Add("schoolCancellation:+20"); }
        if (input.LimitedAvailability) { score += 10; factors.Add("limitedAvailability:+10"); }
        if (input.RegulatoryPriority) { score += 30; factors.Add("regulatoryPriority:+30"); }
        if (input.CommercialPriority) { score += 5; factors.Add("commercialPriority:+5"); }
        if (input.ManualAdjustment != 0)
        {
            score += input.ManualAdjustment;
            string sign = input.ManualAdjustment > 0 ? "+" : string.Empty;
            factors.Add($"manual:{sign}{input.ManualAdjustment}:{Normalize(input.ManualAdjustmentReason!, 120)}");
        }

        score = Math.Clamp(score, 0, 100);
        if (factors.Count == 0) factors.Add("standard:0");
        return Result.Success(new WaitingListPriorityResult(score, string.Join(';', factors)));
    }

    public static int CalculateEffectiveScore(int baseScore, DateTimeOffset createdAtUtc, DateTimeOffset nowUtc)
    {
        int aging = Math.Clamp((int)Math.Floor((nowUtc - createdAtUtc).TotalDays), 0, 20);
        return Math.Clamp(baseScore + aging, 0, 100);
    }

    private static string Normalize(string value, int maxLength)
    {
        string normalized = value.Trim().Replace(';', ',');
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
