using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Domain.JobPositions;
public static class JobPositionErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Workforce.JobPosition.InvalidIdentifier", "errors.workforce.jobPosition.invalidIdentifier");
    public static readonly Error InvalidOrganization = Error.Validation("Workforce.JobPosition.InvalidOrganization", "errors.workforce.jobPosition.invalidOrganization");
    public static readonly Error CodeRequired = Error.Validation("Workforce.JobPosition.CodeRequired", "errors.workforce.jobPosition.codeRequired");
    public static readonly Error CodeTooLong = Error.Validation("Workforce.JobPosition.CodeTooLong", "errors.workforce.jobPosition.codeTooLong");
    public static readonly Error NameRequired = Error.Validation("Workforce.JobPosition.NameRequired", "errors.workforce.jobPosition.nameRequired");
    public static readonly Error NameTooLong = Error.Validation("Workforce.JobPosition.NameTooLong", "errors.workforce.jobPosition.nameTooLong");
    public static readonly Error DuplicateCode = Error.Conflict("Workforce.JobPosition.DuplicateCode", "errors.workforce.jobPosition.duplicateCode");
    public static readonly Error NotFound = Error.NotFound("Workforce.JobPosition.NotFound", "errors.workforce.jobPosition.notFound");
    public static readonly Error Inactive = Error.Conflict("Workforce.JobPosition.Inactive", "errors.workforce.jobPosition.inactive");
    public static readonly Error AlreadyInactive = Error.Conflict("Workforce.JobPosition.AlreadyInactive", "errors.workforce.jobPosition.alreadyInactive");
    public static readonly Error AlreadyActive = Error.Conflict("Workforce.JobPosition.AlreadyActive", "errors.workforce.jobPosition.alreadyActive");
}

public static class EmployeeJobPositionAssignmentErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Workforce.JobPositionAssignment.InvalidIdentifier", "errors.workforce.jobPositionAssignment.invalidIdentifier");
    public static readonly Error InvalidPeriod = Error.Validation("Workforce.JobPositionAssignment.InvalidPeriod", "errors.workforce.jobPositionAssignment.invalidPeriod");
    public static readonly Error EmployeeEnded = Error.Conflict("Workforce.JobPositionAssignment.EmployeeEnded", "errors.workforce.jobPositionAssignment.employeeEnded");
    public static readonly Error PeriodOverlap = Error.Conflict("Workforce.JobPositionAssignment.PeriodOverlap", "errors.workforce.jobPositionAssignment.periodOverlap");
    public static readonly Error PrimaryPeriodOverlap = Error.Conflict("Workforce.JobPositionAssignment.PrimaryPeriodOverlap", "errors.workforce.jobPositionAssignment.primaryPeriodOverlap");
    public static readonly Error BranchAssignmentRequired = Error.Conflict("Workforce.JobPositionAssignment.BranchAssignmentRequired", "errors.workforce.jobPositionAssignment.branchAssignmentRequired");
    public static readonly Error NotFound = Error.NotFound("Workforce.JobPositionAssignment.NotFound", "errors.workforce.jobPositionAssignment.notFound");
    public static readonly Error InvalidTransition = Error.Conflict("Workforce.JobPositionAssignment.InvalidTransition", "errors.workforce.jobPositionAssignment.invalidTransition");
}
