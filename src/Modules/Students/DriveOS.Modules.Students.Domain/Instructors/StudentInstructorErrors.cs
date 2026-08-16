using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.Instructors;

public static class StudentInstructorErrors
{
    public static readonly Error InvalidOwner = Error.Validation(
        "Students.Instructors.InvalidOwner",
        "errors.students.instructors.invalidOwner"
    );
    public static readonly Error InvalidAssignment = Error.Validation(
        "Students.Instructors.InvalidAssignment",
        "errors.students.instructors.invalidAssignment"
    );
    public static readonly Error ReasonRequired = Error.Validation(
        "Students.Instructors.ReasonRequired",
        "errors.students.instructors.reasonRequired"
    );
    public static readonly Error PrimaryAlreadyExists = Error.Conflict(
        "Students.Instructors.PrimaryAlreadyExists",
        "errors.students.instructors.primaryAlreadyExists"
    );
    public static readonly Error AssignmentNotFound = Error.NotFound(
        "Students.Instructors.AssignmentNotFound",
        "errors.students.instructors.assignmentNotFound"
    );
    public static readonly Error InstructorNotEligible = Error.Conflict(
        "Students.Instructors.InstructorNotEligible",
        "errors.students.instructors.instructorNotEligible"
    );
}
