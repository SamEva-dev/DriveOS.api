using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.TrainingPaths.Events;

public sealed record TrainingPathCreatedDomainEvent(
    TrainingPathId TrainingPathId,
    OrganizationId OrganizationId,
    PersonId StudentId,
    CurriculumVersionId CurriculumVersionId,
    TrainingMode TrainingMode) : DomainEvent;

public sealed record TrainingPathMarkedReadyDomainEvent(
    TrainingPathId TrainingPathId,
    OrganizationId OrganizationId,
    PersonId StudentId) : DomainEvent;

public sealed record TrainingPathActivatedDomainEvent(
    TrainingPathId TrainingPathId,
    OrganizationId OrganizationId,
    PersonId StudentId,
    DateTimeOffset ActivatedAtUtc,
    UserId ActivatedByUserId) : DomainEvent;

public sealed record TrainingPathSuspendedDomainEvent(
    TrainingPathId TrainingPathId,
    OrganizationId OrganizationId,
    PersonId StudentId,
    string Reason) : DomainEvent;

public sealed record TrainingPathReactivatedDomainEvent(
    TrainingPathId TrainingPathId,
    OrganizationId OrganizationId,
    PersonId StudentId) : DomainEvent;

public sealed record TrainingPathCompletedDomainEvent(
    TrainingPathId TrainingPathId,
    OrganizationId OrganizationId,
    PersonId StudentId,
    DateTimeOffset CompletedAtUtc) : DomainEvent;

public sealed record TrainingPathCancelledDomainEvent(
    TrainingPathId TrainingPathId,
    OrganizationId OrganizationId,
    PersonId StudentId,
    string Reason) : DomainEvent;

public sealed record TrainingPathMilestoneAddedDomainEvent(
    TrainingPathId TrainingPathId,
    TrainingPathMilestoneId MilestoneId,
    string Code,
    int Order) : DomainEvent;

public sealed record TrainingPathMilestoneCompletedDomainEvent(
    TrainingPathId TrainingPathId,
    TrainingPathMilestoneId MilestoneId,
    UserId CompletedByUserId,
    DateTimeOffset CompletedAtUtc) : DomainEvent;

public sealed record TrainingPathMilestoneStartedDomainEvent(
    TrainingPathId TrainingPathId,
    TrainingPathMilestoneId MilestoneId) : DomainEvent;

public sealed record TrainingPathMilestoneCancelledDomainEvent(
    TrainingPathId TrainingPathId,
    TrainingPathMilestoneId MilestoneId) : DomainEvent;
