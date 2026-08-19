using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;

public sealed record CreateTrainingPathCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    CurriculumVersionId CurriculumVersionId,
    int TrainingMode,
    DateOnly StartDate,
    DateOnly? TargetCompletionDate,
    decimal? EstimatedPracticalHours,
    UserId ActorUserId) : ICommand<TrainingPathId>;

public interface ITrainingPathStudentGateway
{
    Task<bool> ExistsAsync(OrganizationId organizationId, PersonId studentId, CancellationToken cancellationToken = default);
}

public sealed record CurriculumVersionEligibility(
    CurriculumVersionId VersionId,
    CurriculumId CurriculumId,
    string CurriculumCode,
    string CurriculumName,
    int VersionNumber,
    string CountryCode,
    string LicenseCategoryCode,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo);

public interface ICurriculumVersionEligibilityService
{
    Task<CurriculumVersionEligibility?> GetPublishedAsync(
        OrganizationId organizationId,
        CurriculumVersionId versionId,
        DateOnly pathStartDate,
        CancellationToken cancellationToken = default);
}

public static class CreateTrainingPathErrors
{
    public static readonly Error StudentNotFound = Error.NotFound(
        "CurriculumPedagogy.TrainingPath.Student.NotFound",
        "errors.curriculumPedagogy.trainingPath.student.notFound");

    public static readonly Error PublishedCurriculumVersionNotFound = Error.NotFound(
        "CurriculumPedagogy.TrainingPath.CurriculumVersion.PublishedNotFound",
        "errors.curriculumPedagogy.trainingPath.curriculumVersion.publishedNotFound");

    public static readonly Error AlreadyExists = Error.Conflict(
        "CurriculumPedagogy.TrainingPath.AlreadyExists",
        "errors.curriculumPedagogy.trainingPath.alreadyExists");
}

public sealed record TrainingPathMilestoneResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    int Order,
    DateOnly? TargetDate,
    string Status,
    DateTimeOffset? CompletedAtUtc);

public sealed record TrainingPathListItem(
    Guid Id,
    Guid StudentId,
    Guid CurriculumVersionId,
    string CurriculumCode,
    string CurriculumName,
    int CurriculumVersionNumber,
    string CountryCode,
    string LicenseCategoryCode,
    string TrainingMode,
    DateOnly StartDate,
    DateOnly? TargetCompletionDate,
    decimal? EstimatedPracticalHours,
    string Status,
    DateTimeOffset CreatedAtUtc);

public sealed record TrainingPathDetailResponse(
    Guid Id,
    Guid StudentId,
    Guid CurriculumVersionId,
    Guid CurriculumId,
    string CurriculumCode,
    string CurriculumName,
    int CurriculumVersionNumber,
    string CountryCode,
    string LicenseCategoryCode,
    string TrainingMode,
    DateOnly StartDate,
    DateOnly? TargetCompletionDate,
    decimal? EstimatedPracticalHours,
    string Status,
    DateTimeOffset? ActivatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? SuspendedAtUtc,
    string? SuspensionReason,
    DateTimeOffset? CancelledAtUtc,
    string? CancellationReason,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyCollection<TrainingPathMilestoneResponse> Milestones);

public interface ITrainingPathReadService
{
    Task<IReadOnlyCollection<TrainingPathListItem>> ListForStudentAsync(
        OrganizationId organizationId,
        PersonId studentId,
        CancellationToken cancellationToken = default);

    Task<TrainingPathDetailResponse?> GetAsync(
        OrganizationId organizationId,
        TrainingPathId trainingPathId,
        CancellationToken cancellationToken = default);
}
