using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula;

public static class CurriculumErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation(
        "CurriculumPedagogy.Curriculum.Id.Invalid",
        "errors.curriculumPedagogy.curriculum.id.invalid");

    public static readonly Error InvalidOrganization = Error.Validation(
        "CurriculumPedagogy.Curriculum.Organization.Invalid",
        "errors.curriculumPedagogy.curriculum.organization.invalid");

    public static readonly Error InvalidCode = Error.Validation(
        "CurriculumPedagogy.Curriculum.Code.Invalid",
        "errors.curriculumPedagogy.curriculum.code.invalid");

    public static readonly Error InvalidName = Error.Validation(
        "CurriculumPedagogy.Curriculum.Name.Invalid",
        "errors.curriculumPedagogy.curriculum.name.invalid");

    public static readonly Error InvalidDescription = Error.Validation(
        "CurriculumPedagogy.Curriculum.Description.Invalid",
        "errors.curriculumPedagogy.curriculum.description.invalid");

    public static readonly Error InvalidCountryCode = Error.Validation(
        "CurriculumPedagogy.Curriculum.CountryCode.Invalid",
        "errors.curriculumPedagogy.curriculum.countryCode.invalid");

    public static readonly Error InvalidLicenseCategoryCode = Error.Validation(
        "CurriculumPedagogy.Curriculum.LicenseCategoryCode.Invalid",
        "errors.curriculumPedagogy.curriculum.licenseCategoryCode.invalid");

    public static readonly Error ModificationNotAllowed = Error.Conflict(
        "CurriculumPedagogy.Curriculum.Modification.NotAllowed",
        "errors.curriculumPedagogy.curriculum.modification.notAllowed");

    public static readonly Error ArchiveNotAllowed = Error.Conflict(
        "CurriculumPedagogy.Curriculum.Archive.NotAllowed",
        "errors.curriculumPedagogy.curriculum.archive.notAllowed");

    public static readonly Error VersionInvalid = Error.Validation(
        "CurriculumPedagogy.Curriculum.Version.Invalid",
        "errors.curriculumPedagogy.curriculum.version.invalid");

    public static readonly Error VersionEffectivePeriodInvalid = Error.Validation(
        "CurriculumPedagogy.Curriculum.Version.EffectivePeriod.Invalid",
        "errors.curriculumPedagogy.curriculum.version.effectivePeriod.invalid");

    public static readonly Error VersionChangeSummaryInvalid = Error.Validation(
        "CurriculumPedagogy.Curriculum.Version.ChangeSummary.Invalid",
        "errors.curriculumPedagogy.curriculum.version.changeSummary.invalid");

    public static readonly Error VersionCreationNotAllowed = Error.Conflict(
        "CurriculumPedagogy.Curriculum.Version.Creation.NotAllowed",
        "errors.curriculumPedagogy.curriculum.version.creation.notAllowed");

    public static readonly Error NotFound = Error.NotFound(
        "CurriculumPedagogy.Curriculum.NotFound",
        "errors.curriculumPedagogy.curriculum.notFound");

    public static readonly Error CodeAlreadyExists = Error.Conflict(
        "CurriculumPedagogy.Curriculum.Code.AlreadyExists",
        "errors.curriculumPedagogy.curriculum.code.alreadyExists");

    public static readonly Error VersionNotFound = Error.NotFound(
        "CurriculumPedagogy.Curriculum.Version.NotFound",
        "errors.curriculumPedagogy.curriculum.version.notFound");

    public static readonly Error VersionStructureModificationNotAllowed = Error.Conflict(
        "CurriculumPedagogy.Curriculum.Version.Structure.ModificationNotAllowed",
        "errors.curriculumPedagogy.curriculum.version.structure.modificationNotAllowed");

    public static readonly Error ModuleInvalidIdentifier = Error.Validation(
        "CurriculumPedagogy.Curriculum.Module.Identifier.Invalid",
        "errors.curriculumPedagogy.curriculum.module.identifier.invalid");

    public static readonly Error ModuleInvalidCode = Error.Validation(
        "CurriculumPedagogy.Curriculum.Module.Code.Invalid",
        "errors.curriculumPedagogy.curriculum.module.code.invalid");

    public static readonly Error ModuleInvalidName = Error.Validation(
        "CurriculumPedagogy.Curriculum.Module.Name.Invalid",
        "errors.curriculumPedagogy.curriculum.module.name.invalid");

    public static readonly Error ModuleInvalidDescription = Error.Validation(
        "CurriculumPedagogy.Curriculum.Module.Description.Invalid",
        "errors.curriculumPedagogy.curriculum.module.description.invalid");

    public static readonly Error ModuleInvalidOrder = Error.Validation(
        "CurriculumPedagogy.Curriculum.Module.Order.Invalid",
        "errors.curriculumPedagogy.curriculum.module.order.invalid");

    public static readonly Error ModuleCodeAlreadyExists = Error.Conflict(
        "CurriculumPedagogy.Curriculum.Module.Code.AlreadyExists",
        "errors.curriculumPedagogy.curriculum.module.code.alreadyExists");

    public static readonly Error ModuleOrderAlreadyExists = Error.Conflict(
        "CurriculumPedagogy.Curriculum.Module.Order.AlreadyExists",
        "errors.curriculumPedagogy.curriculum.module.order.alreadyExists");

    public static readonly Error ModuleNotFound = Error.NotFound(
        "CurriculumPedagogy.Curriculum.Module.NotFound",
        "errors.curriculumPedagogy.curriculum.module.notFound");

    public static readonly Error ModuleHasCompetencies = Error.Conflict(
        "CurriculumPedagogy.Curriculum.Module.HasCompetencies",
        "errors.curriculumPedagogy.curriculum.module.hasCompetencies");

    public static readonly Error CompetencyInvalidIdentifier = Error.Validation(
        "CurriculumPedagogy.Curriculum.Competency.Identifier.Invalid",
        "errors.curriculumPedagogy.curriculum.competency.identifier.invalid");

    public static readonly Error CompetencyInvalidCode = Error.Validation(
        "CurriculumPedagogy.Curriculum.Competency.Code.Invalid",
        "errors.curriculumPedagogy.curriculum.competency.code.invalid");

    public static readonly Error CompetencyInvalidName = Error.Validation(
        "CurriculumPedagogy.Curriculum.Competency.Name.Invalid",
        "errors.curriculumPedagogy.curriculum.competency.name.invalid");

    public static readonly Error CompetencyInvalidDescription = Error.Validation(
        "CurriculumPedagogy.Curriculum.Competency.Description.Invalid",
        "errors.curriculumPedagogy.curriculum.competency.description.invalid");

    public static readonly Error CompetencyInvalidLearningObjective = Error.Validation(
        "CurriculumPedagogy.Curriculum.Competency.LearningObjective.Invalid",
        "errors.curriculumPedagogy.curriculum.competency.learningObjective.invalid");

    public static readonly Error CompetencyInvalidOrder = Error.Validation(
        "CurriculumPedagogy.Curriculum.Competency.Order.Invalid",
        "errors.curriculumPedagogy.curriculum.competency.order.invalid");

    public static readonly Error CompetencyCodeAlreadyExists = Error.Conflict(
        "CurriculumPedagogy.Curriculum.Competency.Code.AlreadyExists",
        "errors.curriculumPedagogy.curriculum.competency.code.alreadyExists");

    public static readonly Error CompetencyOrderAlreadyExists = Error.Conflict(
        "CurriculumPedagogy.Curriculum.Competency.Order.AlreadyExists",
        "errors.curriculumPedagogy.curriculum.competency.order.alreadyExists");

    public static readonly Error CompetencyNotFound = Error.NotFound(
        "CurriculumPedagogy.Curriculum.Competency.NotFound",
        "errors.curriculumPedagogy.curriculum.competency.notFound");
    public static readonly Error VersionPublishNotAllowed = Error.Conflict(
        "CurriculumPedagogy.Curriculum.Version.Publish.NotAllowed",
        "errors.curriculumPedagogy.curriculum.version.publish.notAllowed");

    public static readonly Error VersionEmpty = Error.Conflict(
        "CurriculumPedagogy.Curriculum.Version.Empty",
        "errors.curriculumPedagogy.curriculum.version.empty");

    public static readonly Error VersionEffectivePeriodOverlaps = Error.Conflict(
        "CurriculumPedagogy.Curriculum.Version.EffectivePeriod.Overlaps",
        "errors.curriculumPedagogy.curriculum.version.effectivePeriod.overlaps");

}
