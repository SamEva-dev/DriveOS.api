using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;

public sealed record MarkTrainingPathReadyCommand(
    OrganizationId OrganizationId,
    TrainingPathId TrainingPathId,
    UserId ActorUserId) : ICommand;

public sealed record ActivateTrainingPathCommand(
    OrganizationId OrganizationId,
    TrainingPathId TrainingPathId,
    UserId ActorUserId) : ICommand;

public sealed record SuspendTrainingPathCommand(
    OrganizationId OrganizationId,
    TrainingPathId TrainingPathId,
    string Reason,
    UserId ActorUserId) : ICommand;

public sealed record ReactivateTrainingPathCommand(
    OrganizationId OrganizationId,
    TrainingPathId TrainingPathId,
    UserId ActorUserId) : ICommand;

public sealed record CompleteTrainingPathCommand(
    OrganizationId OrganizationId,
    TrainingPathId TrainingPathId,
    UserId ActorUserId) : ICommand;

public sealed record CancelTrainingPathCommand(
    OrganizationId OrganizationId,
    TrainingPathId TrainingPathId,
    string Reason,
    UserId ActorUserId) : ICommand;

public sealed record AddTrainingPathMilestoneCommand(
    OrganizationId OrganizationId,
    TrainingPathId TrainingPathId,
    string Code,
    string Name,
    string? Description,
    int Order,
    DateOnly? TargetDate,
    UserId ActorUserId) : ICommand<TrainingPathMilestoneId>;

public sealed record StartTrainingPathMilestoneCommand(
    OrganizationId OrganizationId,
    TrainingPathId TrainingPathId,
    TrainingPathMilestoneId MilestoneId,
    UserId ActorUserId) : ICommand;

public sealed record CompleteTrainingPathMilestoneCommand(
    OrganizationId OrganizationId,
    TrainingPathId TrainingPathId,
    TrainingPathMilestoneId MilestoneId,
    UserId ActorUserId) : ICommand;

public sealed record CancelTrainingPathMilestoneCommand(
    OrganizationId OrganizationId,
    TrainingPathId TrainingPathId,
    TrainingPathMilestoneId MilestoneId,
    UserId ActorUserId) : ICommand;
