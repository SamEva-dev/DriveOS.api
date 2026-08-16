using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.ExternalTransfers;

public static class ExternalTransferErrors
{
    public static readonly Error InvalidOwner = Error.Validation(
        "Students.ExternalTransfer.InvalidOwner",
        "errors.students.externalTransfer.invalidOwner"
    );
    public static readonly Error InvalidRequest = Error.Validation(
        "Students.ExternalTransfer.InvalidRequest",
        "errors.students.externalTransfer.invalidRequest"
    );
    public static readonly Error SameOrganization = Error.Conflict(
        "Students.ExternalTransfer.SameOrganization",
        "errors.students.externalTransfer.sameOrganization"
    );
    public static readonly Error ConsentRequired = Error.Conflict(
        "Students.ExternalTransfer.ConsentRequired",
        "errors.students.externalTransfer.consentRequired"
    );
    public static readonly Error FinancialReviewRequired = Error.Conflict(
        "Students.ExternalTransfer.FinancialReviewRequired",
        "errors.students.externalTransfer.financialReviewRequired"
    );
    public static readonly Error InvalidTransition = Error.Conflict(
        "Students.ExternalTransfer.InvalidTransition",
        "errors.students.externalTransfer.invalidTransition"
    );
    public static readonly Error TransferNotFound = Error.NotFound(
        "Students.ExternalTransfer.NotFound",
        "errors.students.externalTransfer.notFound"
    );
    public static readonly Error ActiveTransferExists = Error.Conflict(
        "Students.ExternalTransfer.ActiveTransferExists",
        "errors.students.externalTransfer.activeTransferExists"
    );
    public static readonly Error RelationshipRequired = Error.Conflict(
        "Students.ExternalTransfer.RelationshipRequired",
        "errors.students.externalTransfer.relationshipRequired"
    );
    public static readonly Error CountryRuleViolation = Error.Conflict(
        "Students.ExternalTransfer.CountryRuleViolation",
        "errors.students.externalTransfer.countryRuleViolation"
    );
}
