using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.Relationships;

public static class StudentRelationshipErrors
{
    public static readonly Error InvalidOwner = Error.Validation(
        "Students.Relationship.Owner.Invalid",
        "errors.students.relationship.owner.invalid"
    );
    public static readonly Error RequiredData = Error.Validation(
        "Students.Relationship.Data.Required",
        "errors.students.relationship.data.required"
    );
    public static readonly Error InvalidPeriod = Error.Validation(
        "Students.Relationship.Period.Invalid",
        "errors.students.relationship.period.invalid"
    );
    public static readonly Error InvalidPrimaryPayer = Error.Validation(
        "Students.Relationship.PrimaryPayer.Invalid",
        "errors.students.relationship.primaryPayer.invalid"
    );
    public static readonly Error NotFound = Error.NotFound(
        "Students.Relationship.NotFound",
        "errors.students.relationship.notFound"
    );
    public static readonly Error NotActive = Error.Conflict(
        "Students.Relationship.NotActive",
        "errors.students.relationship.notActive"
    );
    public static readonly Error Revoked = Error.Conflict(
        "Students.Relationship.Revoked",
        "errors.students.relationship.revoked"
    );
    public static readonly Error ReasonRequired = Error.Validation(
        "Students.Relationship.Reason.Required",
        "errors.students.relationship.reason.required"
    );
    public static readonly Error InvitationContactRequired = Error.Validation(
        "Students.Relationship.InvitationContact.Required",
        "errors.students.relationship.invitationContact.required"
    );
}
