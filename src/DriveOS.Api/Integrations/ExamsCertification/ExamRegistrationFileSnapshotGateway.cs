using System.Text.Json;
using DriveOS.Modules.CurriculumPedagogy.Application.Readiness;
using DriveOS.Modules.ExamsCertification.Application.Registrations.File;
using DriveOS.Modules.ExamsCertification.Application.Readiness;
using DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.File;
using DriveOS.Modules.Students.Application.Documents;
using DriveOS.Modules.Students.Application.Students.Identity;
using DriveOS.Modules.Students.Domain.Documents;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.ExamsCertification;

internal sealed class ExamRegistrationFileSnapshotGateway(
    IStudentIdentityService identities,
    IStudentDocumentService documents,
    IPedagogicalReadinessReadService pedagogy,
    IExamReadinessOpinionRepository opinions,
    IRegulatoryExamFileRequirementGateway regulatory) : IExamRegistrationFileSnapshotGateway
{
    public async Task<Result<ExamRegistrationFileSourceSnapshot>> BuildAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        string examType,
        string licenseCategory,
        CancellationToken cancellationToken = default)
    {
        StudentIdentityResponse? identity = await identities.GetAsync(organizationId, studentId, cancellationToken);
        if (identity is null)
            return Result.Failure<ExamRegistrationFileSourceSnapshot>(ExamReadinessApplicationErrors.StudentNotFound);

        StudentDocumentListResponse? documentList = await documents.GetAsync(
            new GetStudentDocumentsQuery(organizationId, studentId, null), cancellationToken);
        if (documentList is null)
            return Result.Failure<ExamRegistrationFileSourceSnapshot>(ExamReadinessApplicationErrors.StudentNotFound);

        PedagogicalReadinessCheckResponse? pedagogical = await pedagogy.GetAsync(organizationId, trainingPathId, cancellationToken);
        if (pedagogical is null || pedagogical.StudentId != studentId.Value)
            return Result.Failure<ExamRegistrationFileSourceSnapshot>(ExamReadinessApplicationErrors.TrainingPathNotFound);

        IReadOnlyList<ExamReadinessOpinion> opinionItems = await opinions.ListAsync(
            organizationId, studentId, trainingPathId, cancellationToken);

        bool identityVerified = identity.VerificationStatus is IdentityVerificationStatus.DocumentVerified or IdentityVerificationStatus.ExternallyVerified;
        StudentDocumentItem? officialDocument = LatestApproved(documentList.Items, StudentDocumentCategory.Identity)
            ?? LatestApproved(documentList.Items, StudentDocumentCategory.RegulatoryEvidence);
        StudentDocumentItem? photograph = LatestApproved(documentList.Items, StudentDocumentCategory.Photograph);

        ExamReadinessOpinion[] latestOpinions = opinionItems
            .GroupBy(x => x.AuthorId)
            .Select(x => x.OrderByDescending(y => y.Version).First())
            .ToArray();
        bool favorableOpinion = latestOpinions.Length > 0
            && latestOpinions.All(x => x.Opinion is ExamReadinessOpinionType.Favorable or ExamReadinessOpinionType.FavorableWithReservations)
            && latestOpinions.Any(x => x.Opinion == ExamReadinessOpinionType.Favorable);

        bool requiredTrainingSatisfied = pedagogical.Blockers.Count == 0
            && pedagogical.LatestDecision?.Decision is "Ready" or "ReadyWithConditions";

        Result<RegulatoryExamFileRequirement> regulatoryResult = await regulatory.EvaluateAsync(
            organizationId, studentId, trainingPathId, identity.CountryCode, examType, licenseCategory, cancellationToken);
        if (regulatoryResult.IsFailure)
            return Result.Failure<ExamRegistrationFileSourceSnapshot>(regulatoryResult.Error);

        ExamRegistrationRequirementStatus regulatoryStatus = regulatoryResult.Value.Status;
        string? regulatoryEvidence = regulatoryResult.Value.Evidence;

        var evidence = new List<ExamRegistrationFileSourceEvidence>
        {
            new(ExamRegistrationRequirementCodes.IdentityVerified, "Students", identity.VerifiedAtUtc?.ToString("O")),
            new(ExamRegistrationRequirementCodes.OfficialDocument, "Students.Documents", DocumentEvidence(officialDocument)),
            new(ExamRegistrationRequirementCodes.Photograph, "Students.Documents", DocumentEvidence(photograph)),
            new(ExamRegistrationRequirementCodes.PedagogicalOpinion, "ExamsCertification", latestOpinions.Length == 0 ? null : string.Join(';', latestOpinions.Select(x => $"{x.AuthorId.Value}:{x.Opinion}:v{x.Version}"))),
            new(ExamRegistrationRequirementCodes.RequiredTraining, "CurriculumPedagogy", $"coverage={pedagogical.EvaluatedRequiredCompetencies}/{pedagogical.RequiredCompetencies};decision={pedagogical.LatestDecision?.Decision}"),
            new(ExamRegistrationRequirementCodes.RegulatoryTrainingRecord, "RegulatoryTrainingRecord", regulatoryEvidence)
        };

        string officialDataJson = JsonSerializer.Serialize(new
        {
            identity.LegalFirstName,
            identity.LegalLastName,
            identity.BirthDate,
            identity.BirthPlace,
            identity.Nationality,
            identity.CountryCode,
            IdentityVerificationStatus = identity.VerificationStatus.ToString(),
            IdentityVerifiedAtUtc = identity.VerifiedAtUtc
        });

        return Result.Success(new ExamRegistrationFileSourceSnapshot(
            identityVerified,
            officialDocument is not null,
            photograph is not null,
            favorableOpinion,
            requiredTrainingSatisfied,
            regulatoryResult.Value.Required,
            regulatoryStatus,
            regulatoryEvidence,
            officialDataJson,
            evidence));
    }

    private static StudentDocumentItem? LatestApproved(IReadOnlyList<StudentDocumentItem> items, StudentDocumentCategory category) =>
        items.Where(x => x.Category == category && x.Status == StudentDocumentStatus.Approved)
            .OrderByDescending(x => x.UploadedAtUtc)
            .FirstOrDefault();

    private static string? DocumentEvidence(StudentDocumentItem? item) => item is null
        ? null
        : $"documentId={item.Id};type={item.DocumentType};version={item.CurrentVersion};expiresOn={item.ExpiresOn:yyyy-MM-dd}";
}
