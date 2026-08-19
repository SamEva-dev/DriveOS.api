using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.TrainingPaths;

public static class TrainingPathErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation(
        "CurriculumPedagogy.TrainingPath.Id.Invalid",
        "errors.curriculumPedagogy.trainingPath.id.invalid");

    public static readonly Error InvalidOrganization = Error.Validation(
        "CurriculumPedagogy.TrainingPath.Organization.Invalid",
        "errors.curriculumPedagogy.trainingPath.organization.invalid");

    public static readonly Error InvalidStudent = Error.Validation(
        "CurriculumPedagogy.TrainingPath.Student.Invalid",
        "errors.curriculumPedagogy.trainingPath.student.invalid");

    public static readonly Error InvalidCurriculumVersion = Error.Validation(
        "CurriculumPedagogy.TrainingPath.CurriculumVersion.Invalid",
        "errors.curriculumPedagogy.trainingPath.curriculumVersion.invalid");

    public static readonly Error InvalidTrainingMode = Error.Validation(
        "CurriculumPedagogy.TrainingPath.TrainingMode.Invalid",
        "errors.curriculumPedagogy.trainingPath.trainingMode.invalid");

    public static readonly Error InvalidStartDate = Error.Validation(
        "CurriculumPedagogy.TrainingPath.StartDate.Invalid",
        "errors.curriculumPedagogy.trainingPath.startDate.invalid");

    public static readonly Error InvalidTargetDate = Error.Validation(
        "CurriculumPedagogy.TrainingPath.TargetDate.Invalid",
        "errors.curriculumPedagogy.trainingPath.targetDate.invalid");

    public static readonly Error InvalidEstimatedPracticalHours = Error.Validation(
        "CurriculumPedagogy.TrainingPath.EstimatedPracticalHours.Invalid",
        "errors.curriculumPedagogy.trainingPath.estimatedPracticalHours.invalid");

    public static readonly Error ModificationNotAllowed = Error.Conflict(
        "CurriculumPedagogy.TrainingPath.Modification.NotAllowed",
        "errors.curriculumPedagogy.trainingPath.modification.notAllowed");

    public static readonly Error MarkReadyNotAllowed = Error.Conflict(
        "CurriculumPedagogy.TrainingPath.MarkReady.NotAllowed",
        "errors.curriculumPedagogy.trainingPath.markReady.notAllowed");

    public static readonly Error ActivationNotAllowed = Error.Conflict(
        "CurriculumPedagogy.TrainingPath.Activation.NotAllowed",
        "errors.curriculumPedagogy.trainingPath.activation.notAllowed");

    public static readonly Error SuspensionNotAllowed = Error.Conflict(
        "CurriculumPedagogy.TrainingPath.Suspension.NotAllowed",
        "errors.curriculumPedagogy.trainingPath.suspension.notAllowed");

    public static readonly Error ReactivationNotAllowed = Error.Conflict(
        "CurriculumPedagogy.TrainingPath.Reactivation.NotAllowed",
        "errors.curriculumPedagogy.trainingPath.reactivation.notAllowed");

    public static readonly Error CompletionNotAllowed = Error.Conflict(
        "CurriculumPedagogy.TrainingPath.Completion.NotAllowed",
        "errors.curriculumPedagogy.trainingPath.completion.notAllowed");

    public static readonly Error CancellationNotAllowed = Error.Conflict(
        "CurriculumPedagogy.TrainingPath.Cancellation.NotAllowed",
        "errors.curriculumPedagogy.trainingPath.cancellation.notAllowed");

    public static readonly Error InvalidMilestone = Error.Validation(
        "CurriculumPedagogy.TrainingPath.Milestone.Invalid",
        "errors.curriculumPedagogy.trainingPath.milestone.invalid");

    public static readonly Error MilestoneCodeAlreadyExists = Error.Conflict(
        "CurriculumPedagogy.TrainingPath.Milestone.Code.AlreadyExists",
        "errors.curriculumPedagogy.trainingPath.milestone.code.alreadyExists");

    public static readonly Error MilestoneOrderAlreadyExists = Error.Conflict(
        "CurriculumPedagogy.TrainingPath.Milestone.Order.AlreadyExists",
        "errors.curriculumPedagogy.trainingPath.milestone.order.alreadyExists");

    public static readonly Error MilestoneNotFound = Error.NotFound(
        "CurriculumPedagogy.TrainingPath.Milestone.NotFound",
        "errors.curriculumPedagogy.trainingPath.milestone.notFound");

    public static readonly Error MilestoneCompletionNotAllowed = Error.Conflict(
        "CurriculumPedagogy.TrainingPath.Milestone.Completion.NotAllowed",
        "errors.curriculumPedagogy.trainingPath.milestone.completion.notAllowed");

    public static readonly Error OpenMilestonesRemain = Error.Conflict(
        "CurriculumPedagogy.TrainingPath.OpenMilestonesRemain",
        "errors.curriculumPedagogy.trainingPath.openMilestonesRemain");
}
