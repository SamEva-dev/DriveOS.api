using DriveOS.Modules.Students.Domain.Checklists;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Students.Application.Checklists;

/// <summary>
/// Cross-bounded-context snapshot used by Student Administration to evaluate
/// enrollment prerequisites without taking ownership of the source data.
/// Null means that the source cannot currently determine the prerequisite.
/// </summary>
public sealed record EnrollmentPrerequisiteSnapshot(
    PrerequisiteEvaluation? Contract,
    PrerequisiteEvaluation? InitialPayment,
    PrerequisiteEvaluation? InitialAssessment,
    PrerequisiteEvaluation? LearningPath,
    PrerequisiteEvaluation? StudentAccount
);

public sealed record PrerequisiteEvaluation(
    ChecklistItemStatus Status,
    string? EvidenceReference = null
);

public interface IEnrollmentPrerequisiteSnapshotProvider
{
    Task<EnrollmentPrerequisiteSnapshot> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        DraftEnrollmentId enrollmentId,
        LeadId? sourceLeadId,
        CancellationToken cancellationToken = default);
}
