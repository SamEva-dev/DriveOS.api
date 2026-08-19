using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CurriculumPedagogy.Application.Persistence;
using DriveOS.Modules.CurriculumPedagogy.Domain.TrainingPaths;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;

public sealed class CreateTrainingPathCommandHandler(
    ITrainingPathStudentGateway students,
    ICurriculumVersionEligibilityService curriculumVersions,
    ITrainingPathRepository trainingPaths,
    ICurriculumPedagogyUnitOfWork unitOfWork)
    : ICommandHandler<CreateTrainingPathCommand, TrainingPathId>
{
    public async Task<Result<TrainingPathId>> Handle(CreateTrainingPathCommand command, CancellationToken cancellationToken)
    {
        if (!await students.ExistsAsync(command.OrganizationId, command.StudentId, cancellationToken))
            return Result.Failure<TrainingPathId>(CreateTrainingPathErrors.StudentNotFound);

        CurriculumVersionEligibility? version = await curriculumVersions.GetPublishedAsync(
            command.OrganizationId,
            command.CurriculumVersionId,
            command.StartDate,
            cancellationToken);

        if (version is null)
            return Result.Failure<TrainingPathId>(CreateTrainingPathErrors.PublishedCurriculumVersionNotFound);

        if (await trainingPaths.ExistsOpenForStudentAndVersionAsync(
                command.OrganizationId,
                command.StudentId,
                command.CurriculumVersionId,
                cancellationToken))
        {
            return Result.Failure<TrainingPathId>(CreateTrainingPathErrors.AlreadyExists);
        }

        if (!Enum.IsDefined(typeof(TrainingMode), command.TrainingMode))
            return Result.Failure<TrainingPathId>(TrainingPathErrors.InvalidTrainingMode);

        Result<TrainingPath> result = TrainingPath.Create(
            TrainingPathId.New(),
            command.OrganizationId,
            command.StudentId,
            command.CurriculumVersionId,
            (TrainingMode)command.TrainingMode,
            command.StartDate,
            command.TargetCompletionDate,
            command.EstimatedPracticalHours);

        if (result.IsFailure)
            return Result.Failure<TrainingPathId>(result.Error);

        await trainingPaths.AddAsync(result.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}
