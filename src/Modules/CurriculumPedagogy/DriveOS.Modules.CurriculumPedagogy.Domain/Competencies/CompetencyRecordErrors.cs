using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Competencies;

public static class CompetencyRecordErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation(
        "CurriculumPedagogy.CompetencyRecord.Id.Invalid",
        "errors.curriculumPedagogy.competencyRecord.id.invalid");

    public static readonly Error InvalidOrganization = Error.Validation(
        "CurriculumPedagogy.CompetencyRecord.Organization.Invalid",
        "errors.curriculumPedagogy.competencyRecord.organization.invalid");

    public static readonly Error InvalidTrainingPath = Error.Validation(
        "CurriculumPedagogy.CompetencyRecord.TrainingPath.Invalid",
        "errors.curriculumPedagogy.competencyRecord.trainingPath.invalid");

    public static readonly Error InvalidCurriculumVersion = Error.Validation(
        "CurriculumPedagogy.CompetencyRecord.CurriculumVersion.Invalid",
        "errors.curriculumPedagogy.competencyRecord.curriculumVersion.invalid");

    public static readonly Error InvalidCompetency = Error.Validation(
        "CurriculumPedagogy.CompetencyRecord.Competency.Invalid",
        "errors.curriculumPedagogy.competencyRecord.competency.invalid");

    public static readonly Error InvalidAssessment = Error.Validation(
        "CurriculumPedagogy.CompetencyRecord.Assessment.Invalid",
        "errors.curriculumPedagogy.competencyRecord.assessment.invalid");

    public static readonly Error InvalidLevelCode = Error.Validation(
        "CurriculumPedagogy.CompetencyRecord.LevelCode.Invalid",
        "errors.curriculumPedagogy.competencyRecord.levelCode.invalid");

    public static readonly Error InvalidComment = Error.Validation(
        "CurriculumPedagogy.CompetencyRecord.Comment.Invalid",
        "errors.curriculumPedagogy.competencyRecord.comment.invalid");
}
