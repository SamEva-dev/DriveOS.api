using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.TrainingPaths;

public sealed class TrainingPathMilestone : Entity<TrainingPathMilestoneId>
{
    private TrainingPathMilestone() { }

    private TrainingPathMilestone(
        TrainingPathMilestoneId id,
        TrainingPathId trainingPathId,
        string code,
        string name,
        string? description,
        int order,
        DateOnly? targetDate)
        : base(id)
    {
        TrainingPathId = trainingPathId;
        Code = code;
        Name = name;
        Description = description;
        Order = order;
        TargetDate = targetDate;
        Status = TrainingPathMilestoneStatus.Planned;
    }

    public TrainingPathId TrainingPathId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Order { get; private set; }
    public DateOnly? TargetDate { get; private set; }
    public TrainingPathMilestoneStatus Status { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public UserId? CompletedByUserId { get; private set; }

    internal static Result<TrainingPathMilestone> Create(
        TrainingPathMilestoneId id,
        TrainingPathId trainingPathId,
        string code,
        string name,
        string? description,
        int order,
        DateOnly? targetDate)
    {
        string normalizedCode = (code ?? string.Empty).Trim().ToUpperInvariant();
        string normalizedName = (name ?? string.Empty).Trim();
        string? normalizedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        if (id.IsEmpty || trainingPathId.IsEmpty ||
            normalizedCode.Length is < 1 or > 50 ||
            normalizedName.Length is < 2 or > 200 ||
            normalizedDescription?.Length > 1000 ||
            order <= 0)
        {
            return Result.Failure<TrainingPathMilestone>(TrainingPathErrors.InvalidMilestone);
        }

        return Result.Success(new TrainingPathMilestone(
            id, trainingPathId, normalizedCode, normalizedName, normalizedDescription, order, targetDate));
    }

    internal Result Start()
    {
        if (Status != TrainingPathMilestoneStatus.Planned)
            return Result.Failure(TrainingPathErrors.MilestoneCompletionNotAllowed);

        Status = TrainingPathMilestoneStatus.InProgress;
        return Result.Success();
    }

    internal Result Complete(UserId actorUserId, DateTimeOffset completedAtUtc)
    {
        if (Status is TrainingPathMilestoneStatus.Completed or TrainingPathMilestoneStatus.Cancelled ||
            actorUserId.IsEmpty || completedAtUtc == default)
        {
            return Result.Failure(TrainingPathErrors.MilestoneCompletionNotAllowed);
        }

        Status = TrainingPathMilestoneStatus.Completed;
        CompletedByUserId = actorUserId;
        CompletedAtUtc = completedAtUtc.ToUniversalTime();
        return Result.Success();
    }

    internal Result Cancel()
    {
        if (Status == TrainingPathMilestoneStatus.Completed)
            return Result.Failure(TrainingPathErrors.MilestoneCompletionNotAllowed);

        Status = TrainingPathMilestoneStatus.Cancelled;
        return Result.Success();
    }
}
