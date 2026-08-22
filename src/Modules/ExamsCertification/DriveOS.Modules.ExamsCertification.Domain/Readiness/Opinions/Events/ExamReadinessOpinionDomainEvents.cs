using DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions.Events;

public sealed record ExamReadinessOpinionSubmittedDomainEvent(
    ExamReadinessOpinionId OpinionId,
    OrganizationId OrganizationId,
    PersonId StudentId,
    TrainingPathId TrainingPathId,
    ExamReadinessOpinionType Opinion,
    UserId AuthorId,
    int Version) : DomainEvent;

public sealed record ExamReadinessSecondOpinionRequestedDomainEvent(
    ExamReadinessOpinionId OpinionId,
    OrganizationId OrganizationId,
    PersonId StudentId,
    TrainingPathId TrainingPathId,
    UserId RequestedByUserId) : DomainEvent;
