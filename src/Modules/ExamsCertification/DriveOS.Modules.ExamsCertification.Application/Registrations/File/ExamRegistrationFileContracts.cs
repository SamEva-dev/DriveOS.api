using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.File;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations.File;

public sealed record ExamRegistrationFileSourceSnapshot(
    bool IdentityVerified,
    bool HasApprovedOfficialDocument,
    bool HasApprovedPhotograph,
    bool HasFavorablePedagogicalOpinion,
    bool RequiredTrainingSatisfied,
    bool RegulatoryTrainingRecordRequired,
    ExamRegistrationRequirementStatus RegulatoryTrainingRecordStatus,
    string? RegulatoryEvidence,
    string? OfficialDataJson,
    IReadOnlyList<ExamRegistrationFileSourceEvidence> Evidence);

public sealed record ExamRegistrationFileSourceEvidence(string Code, string Source, string? Evidence);

public interface IExamRegistrationFileSnapshotGateway
{
    Task<Result<ExamRegistrationFileSourceSnapshot>> BuildAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        string examType,
        string licenseCategory,
        CancellationToken cancellationToken = default);
}


public sealed record RegulatoryExamFileRequirement(
    bool Required,
    ExamRegistrationRequirementStatus Status,
    string? Evidence);

public interface IRegulatoryExamFileRequirementGateway
{
    Task<Result<RegulatoryExamFileRequirement>> EvaluateAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        string? countryCode,
        string examType,
        string licenseCategory,
        CancellationToken cancellationToken = default);
}

public sealed record RefreshExamRegistrationFileCommand(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    UserId ActorUserId) : ICommand<ExamRegistrationFileResponse>;

public sealed record UpdateExamRegistrationOfficialDataCommand(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    string CandidateReference,
    UserId ActorUserId) : ICommand<ExamRegistrationFileResponse>;

public sealed record GetExamRegistrationFileQuery(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId) : IQuery<ExamRegistrationFileResponse>;

public sealed record ExamRegistrationFileResponse(
    Guid Id,
    Guid RegistrationId,
    Guid StudentId,
    string Status,
    int CurrentVersion,
    string? CandidateReference,
    DateTimeOffset? LastEvaluatedAtUtc,
    IReadOnlyList<ExamRegistrationFileRevisionResponse> Revisions);

public sealed record ExamRegistrationFileRevisionResponse(
    int Version,
    string? CandidateReference,
    string? OfficialDataJson,
    DateTimeOffset CreatedAtUtc,
    Guid CreatedByUserId,
    IReadOnlyList<ExamRegistrationChecklistItemResponse> Checklist);

public sealed record ExamRegistrationChecklistItemResponse(
    string Code,
    bool Required,
    string Status,
    string MessageKey,
    string? Source,
    string? Evidence);
