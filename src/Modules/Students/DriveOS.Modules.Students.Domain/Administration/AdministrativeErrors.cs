using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.Administration;

public static class AdministrativeErrors
{
    public static readonly Error InvalidOwner = Error.Validation(
        "Students.Administration.Owner.Invalid",
        "errors.students.administration.owner.invalid"
    );
    public static readonly Error InvalidRequirement = Error.Validation(
        "Students.Administration.Requirement.Invalid",
        "errors.students.administration.requirement.invalid"
    );
    public static readonly Error RequirementNotFound = Error.NotFound(
        "Students.Administration.Requirement.NotFound",
        "errors.students.administration.requirement.notFound"
    );
    public static readonly Error InvalidRequirementStatus = Error.Validation(
        "Students.Administration.Requirement.Status.Invalid",
        "errors.students.administration.requirement.status.invalid"
    );
    public static readonly Error DecisionReasonRequired = Error.Validation(
        "Students.Administration.Decision.Reason.Required",
        "errors.students.administration.decision.reason.required"
    );
    public static readonly Error BlockReasonRequired = Error.Validation(
        "Students.Administration.Block.Reason.Required",
        "errors.students.administration.block.reason.required"
    );
    public static readonly Error BlockNotFound = Error.NotFound(
        "Students.Administration.Block.NotFound",
        "errors.students.administration.block.notFound"
    );
    public static readonly Error ExceptionReasonRequired = Error.Validation(
        "Students.Administration.Exception.Reason.Required",
        "errors.students.administration.exception.reason.required"
    );
    public static readonly Error ExceptionNotFound = Error.NotFound(
        "Students.Administration.Exception.NotFound",
        "errors.students.administration.exception.notFound"
    );
    public static readonly Error ExceptionAlreadyDecided = Error.Conflict(
        "Students.Administration.Exception.AlreadyDecided",
        "errors.students.administration.exception.alreadyDecided"
    );
}
