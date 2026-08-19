using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.PedagogicalReviews.Events;

public sealed record PedagogicalReviewRequestedDomainEvent(PedagogicalReviewId ReviewId, OrganizationId OrganizationId, PersonId StudentId, TrainingPathId TrainingPathId, UserId ReviewerId) : DomainEvent;
public sealed record PedagogicalReviewStartedDomainEvent(PedagogicalReviewId ReviewId, TrainingPathId TrainingPathId, UserId ReviewerId) : DomainEvent;
public sealed record PedagogicalReviewCompletedDomainEvent(PedagogicalReviewId ReviewId, OrganizationId OrganizationId, PersonId StudentId, TrainingPathId TrainingPathId, UserId ReviewerId, DateTimeOffset CompletedAtUtc) : DomainEvent;
public sealed record PedagogicalReviewCancelledDomainEvent(PedagogicalReviewId ReviewId, TrainingPathId TrainingPathId, string Reason) : DomainEvent;
