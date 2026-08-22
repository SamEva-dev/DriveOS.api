using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Registrations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.File;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations.File;

public sealed class RefreshExamRegistrationFileCommandHandler(
    IExamRegistrationRepository registrations,
    IExamRegistrationFileRepository files,
    IExamRegistrationFileSnapshotGateway snapshotGateway,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RefreshExamRegistrationFileCommand, ExamRegistrationFileResponse>
{
    public async Task<Result<ExamRegistrationFileResponse>> Handle(RefreshExamRegistrationFileCommand command, CancellationToken cancellationToken)
    {
        ExamRegistration? registration = await registrations.GetByIdAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (registration is null) return Result.Failure<ExamRegistrationFileResponse>(ExamRegistrationErrors.NotFound);

        Result<ExamRegistrationFileSourceSnapshot> snapshot = await snapshotGateway.BuildAsync(
            command.OrganizationId, registration.StudentId, registration.TrainingPathId, registration.ExamType, registration.LicenseCategory, cancellationToken);
        if (snapshot.IsFailure) return Result.Failure<ExamRegistrationFileResponse>(snapshot.Error);

        ExamRegistrationFile? file = await files.GetByRegistrationForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (file is null)
        {
            Result<ExamRegistrationFile> creation = ExamRegistrationFile.Create(
                command.OrganizationId, registration.Id, registration.StudentId, command.ActorUserId, clock.UtcNow);
            if (creation.IsFailure) return Result.Failure<ExamRegistrationFileResponse>(creation.Error);
            file = creation.Value;
            files.Add(file);
        }

        Result<ExamRegistrationFileRevision> refresh = file.Refresh(
            BuildChecklist(snapshot.Value, registration.CandidateReference),
            registration.CandidateReference,
            snapshot.Value.OfficialDataJson,
            command.ActorUserId,
            clock.UtcNow);
        if (refresh.IsFailure) return Result.Failure<ExamRegistrationFileResponse>(refresh.Error);

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(Map(file));
    }

    internal static IReadOnlyCollection<ExamRegistrationChecklistSnapshotItem> BuildChecklist(
        ExamRegistrationFileSourceSnapshot snapshot, string? candidateReference) =>
    [
        Item(ExamRegistrationRequirementCodes.IdentityVerified, true, snapshot.IdentityVerified,
            "exams.registrationFile.identity.compliant", "exams.registrationFile.identity.missing", "Students", Find(snapshot, ExamRegistrationRequirementCodes.IdentityVerified)),
        Item(ExamRegistrationRequirementCodes.OfficialDocument, true, snapshot.HasApprovedOfficialDocument,
            "exams.registrationFile.officialDocument.compliant", "exams.registrationFile.officialDocument.missing", "Students.Documents", Find(snapshot, ExamRegistrationRequirementCodes.OfficialDocument)),
        Item(ExamRegistrationRequirementCodes.Photograph, true, snapshot.HasApprovedPhotograph,
            "exams.registrationFile.photograph.compliant", "exams.registrationFile.photograph.missing", "Students.Documents", Find(snapshot, ExamRegistrationRequirementCodes.Photograph)),
        Item(ExamRegistrationRequirementCodes.PedagogicalOpinion, true, snapshot.HasFavorablePedagogicalOpinion,
            "exams.registrationFile.opinion.compliant", "exams.registrationFile.opinion.missing", "ExamsCertification", Find(snapshot, ExamRegistrationRequirementCodes.PedagogicalOpinion)),
        Item(ExamRegistrationRequirementCodes.RequiredTraining, true, snapshot.RequiredTrainingSatisfied,
            "exams.registrationFile.training.compliant", "exams.registrationFile.training.incomplete", "CurriculumPedagogy", Find(snapshot, ExamRegistrationRequirementCodes.RequiredTraining)),
        Item(ExamRegistrationRequirementCodes.CandidateReference, true, !string.IsNullOrWhiteSpace(candidateReference),
            "exams.registrationFile.candidateReference.compliant", "exams.registrationFile.candidateReference.missing", "ExamsCertification", candidateReference),
        new ExamRegistrationChecklistSnapshotItem(
            ExamRegistrationRequirementCodes.RegulatoryTrainingRecord,
            snapshot.RegulatoryTrainingRecordRequired,
            snapshot.RegulatoryTrainingRecordStatus,
            snapshot.RegulatoryTrainingRecordStatus switch
            {
                ExamRegistrationRequirementStatus.Compliant => "exams.registrationFile.regulatory.compliant",
                ExamRegistrationRequirementStatus.NotApplicable => "exams.registrationFile.regulatory.notApplicable",
                ExamRegistrationRequirementStatus.Warning => "exams.registrationFile.regulatory.warning",
                ExamRegistrationRequirementStatus.Blocked => "exams.registrationFile.regulatory.blocked",
                _ => "exams.registrationFile.regulatory.pending"
            },
            "RegulatoryTrainingRecord",
            snapshot.RegulatoryEvidence)
    ];

    private static ExamRegistrationChecklistSnapshotItem Item(string code, bool required, bool compliant,
        string okKey, string missingKey, string source, string? evidence) => new(
            code, required, compliant ? ExamRegistrationRequirementStatus.Compliant : ExamRegistrationRequirementStatus.Missing,
            compliant ? okKey : missingKey, source, evidence);

    private static string? Find(ExamRegistrationFileSourceSnapshot snapshot, string code) =>
        snapshot.Evidence.FirstOrDefault(x => x.Code == code)?.Evidence;

    internal static ExamRegistrationFileResponse Map(ExamRegistrationFile file) => new(
        file.Id.Value,
        file.RegistrationId.Value,
        file.StudentId.Value,
        file.Status.ToString(),
        file.CurrentVersion,
        file.CurrentRevision?.CandidateReference,
        file.LastEvaluatedAtUtc,
        file.Revisions.OrderByDescending(x => x.Version).Select(x => new ExamRegistrationFileRevisionResponse(
            x.Version, x.CandidateReference, x.OfficialDataJson, x.CreatedAtUtc, x.CreatedByUserId.Value,
            x.Checklist.Select(i => new ExamRegistrationChecklistItemResponse(
                i.Code, i.Required, i.Status.ToString(), i.MessageKey, i.Source, i.Evidence)).ToArray())).ToArray());
}

public sealed class UpdateExamRegistrationOfficialDataCommandHandler(
    IExamRegistrationRepository registrations,
    IExamRegistrationFileRepository files,
    IExamRegistrationFileSnapshotGateway snapshotGateway,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<UpdateExamRegistrationOfficialDataCommand, ExamRegistrationFileResponse>
{
    public async Task<Result<ExamRegistrationFileResponse>> Handle(UpdateExamRegistrationOfficialDataCommand command, CancellationToken cancellationToken)
    {
        ExamRegistration? registration = await registrations.GetByIdForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (registration is null) return Result.Failure<ExamRegistrationFileResponse>(ExamRegistrationErrors.NotFound);

        Result update = registration.UpdateCandidateReference(command.CandidateReference, command.ActorUserId, clock.UtcNow);
        if (update.IsFailure) return Result.Failure<ExamRegistrationFileResponse>(update.Error);

        Result<ExamRegistrationFileSourceSnapshot> snapshot = await snapshotGateway.BuildAsync(
            command.OrganizationId, registration.StudentId, registration.TrainingPathId, registration.ExamType, registration.LicenseCategory, cancellationToken);
        if (snapshot.IsFailure) return Result.Failure<ExamRegistrationFileResponse>(snapshot.Error);

        ExamRegistrationFile? file = await files.GetByRegistrationForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (file is null)
        {
            Result<ExamRegistrationFile> creation = ExamRegistrationFile.Create(
                command.OrganizationId, registration.Id, registration.StudentId, command.ActorUserId, clock.UtcNow);
            if (creation.IsFailure) return Result.Failure<ExamRegistrationFileResponse>(creation.Error);
            file = creation.Value;
            files.Add(file);
        }

        Result<ExamRegistrationFileRevision> refresh = file.Refresh(
            RefreshExamRegistrationFileCommandHandler.BuildChecklist(snapshot.Value, registration.CandidateReference),
            registration.CandidateReference, snapshot.Value.OfficialDataJson, command.ActorUserId, clock.UtcNow);
        if (refresh.IsFailure) return Result.Failure<ExamRegistrationFileResponse>(refresh.Error);

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(RefreshExamRegistrationFileCommandHandler.Map(file));
    }
}

public sealed class GetExamRegistrationFileQueryHandler(IExamRegistrationFileRepository files)
    : IQueryHandler<GetExamRegistrationFileQuery, ExamRegistrationFileResponse>
{
    public async Task<Result<ExamRegistrationFileResponse>> Handle(GetExamRegistrationFileQuery query, CancellationToken cancellationToken)
    {
        ExamRegistrationFile? file = await files.GetByRegistrationAsync(query.OrganizationId, query.RegistrationId, cancellationToken);
        return file is null
            ? Result.Failure<ExamRegistrationFileResponse>(ExamRegistrationFileErrors.NotFound)
            : Result.Success(RefreshExamRegistrationFileCommandHandler.Map(file));
    }
}
