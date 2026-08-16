using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.Branches;

public static class StudentBranchErrors
{
    public static readonly Error InvalidOwner = Error.Validation(
        "Students.Branches.InvalidOwner",
        "errors.students.branches.invalidOwner"
    );
    public static readonly Error InvalidPeriod = Error.Validation(
        "Students.Branches.InvalidPeriod",
        "errors.students.branches.invalidPeriod"
    );
    public static readonly Error ReasonRequired = Error.Validation(
        "Students.Branches.ReasonRequired",
        "errors.students.branches.reasonRequired"
    );
    public static readonly Error PrimaryAlreadyExists = Error.Conflict(
        "Students.Branches.PrimaryAlreadyExists",
        "errors.students.branches.primaryAlreadyExists"
    );
    public static readonly Error AssignmentNotFound = Error.NotFound(
        "Students.Branches.AssignmentNotFound",
        "errors.students.branches.assignmentNotFound"
    );
    public static readonly Error AnalysisRequired = Error.Conflict(
        "Students.Branches.AnalysisRequired",
        "errors.students.branches.analysisRequired"
    );
    public static readonly Error AnalysisExpired = Error.Conflict(
        "Students.Branches.AnalysisExpired",
        "errors.students.branches.analysisExpired"
    );
    public static readonly Error BranchNotEligible = Error.Conflict(
        "Students.Branches.BranchNotEligible",
        "errors.students.branches.branchNotEligible"
    );
}
