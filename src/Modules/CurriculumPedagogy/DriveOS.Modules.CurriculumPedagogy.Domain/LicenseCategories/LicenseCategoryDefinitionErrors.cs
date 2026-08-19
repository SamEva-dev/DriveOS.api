using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.LicenseCategories;

public static class LicenseCategoryDefinitionErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation(
        "CurriculumPedagogy.LicenseCategory.Id.Invalid",
        "errors.curriculumPedagogy.licenseCategory.id.invalid");

    public static readonly Error InvalidOrganization = Error.Validation(
        "CurriculumPedagogy.LicenseCategory.Organization.Invalid",
        "errors.curriculumPedagogy.licenseCategory.organization.invalid");

    public static readonly Error InvalidName = Error.Validation(
        "CurriculumPedagogy.LicenseCategory.Name.Invalid",
        "errors.curriculumPedagogy.licenseCategory.name.invalid");

    public static readonly Error InvalidDescription = Error.Validation(
        "CurriculumPedagogy.LicenseCategory.Description.Invalid",
        "errors.curriculumPedagogy.licenseCategory.description.invalid");

    public static readonly Error ModificationNotAllowed = Error.Conflict(
        "CurriculumPedagogy.LicenseCategory.Modification.NotAllowed",
        "errors.curriculumPedagogy.licenseCategory.modification.notAllowed");

    public static readonly Error ActivationNotAllowed = Error.Conflict(
        "CurriculumPedagogy.LicenseCategory.Activation.NotAllowed",
        "errors.curriculumPedagogy.licenseCategory.activation.notAllowed");

    public static readonly Error ArchiveNotAllowed = Error.Conflict(
        "CurriculumPedagogy.LicenseCategory.Archive.NotAllowed",
        "errors.curriculumPedagogy.licenseCategory.archive.notAllowed");

    public static readonly Error NotFound = Error.NotFound(
        "CurriculumPedagogy.LicenseCategory.NotFound",
        "errors.curriculumPedagogy.licenseCategory.notFound");

    public static readonly Error ScopeAlreadyExists = Error.Conflict(
        "CurriculumPedagogy.LicenseCategory.Scope.AlreadyExists",
        "errors.curriculumPedagogy.licenseCategory.scope.alreadyExists");
}
