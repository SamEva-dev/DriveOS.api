using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Enrollments.StartDirectEnrollment;

public static class DirectEnrollmentErrors
{
    public static readonly Error StudentNotFound = Error.NotFound(
        "Students.DirectEnrollment.Student.NotFound",
        "errors.students.directEnrollment.student.notFound"
    );
    public static readonly Error PossibleDuplicate = Error.Conflict(
        "Students.DirectEnrollment.PossibleDuplicate",
        "errors.students.directEnrollment.possibleDuplicate"
    );
}
