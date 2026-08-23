using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Workforce.Domain.BranchAssignments;

public static class EmployeeBranchAssignmentErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Workforce.BranchAssignment.InvalidIdentifier", "errors.workforce.branchAssignment.invalidIdentifier");
    public static readonly Error BranchRequired = Error.Validation("Workforce.BranchAssignment.BranchRequired", "errors.workforce.branchAssignment.branchRequired");
    public static readonly Error BranchNotFound = Error.NotFound("Workforce.BranchAssignment.BranchNotFound", "errors.workforce.branchAssignment.branchNotFound");
    public static readonly Error BranchClosed = Error.Conflict("Workforce.BranchAssignment.BranchClosed", "errors.workforce.branchAssignment.branchClosed");
    public static readonly Error InvalidPeriod = Error.Validation("Workforce.BranchAssignment.InvalidPeriod", "errors.workforce.branchAssignment.invalidPeriod");
    public static readonly Error SameBranchPeriodOverlap = Error.Conflict("Workforce.BranchAssignment.SameBranchPeriodOverlap", "errors.workforce.branchAssignment.sameBranchPeriodOverlap");
    public static readonly Error PrimaryPeriodOverlap = Error.Conflict("Workforce.BranchAssignment.PrimaryPeriodOverlap", "errors.workforce.branchAssignment.primaryPeriodOverlap");
    public static readonly Error NotFound = Error.NotFound("Workforce.BranchAssignment.NotFound", "errors.workforce.branchAssignment.notFound");
    public static readonly Error NotEditable = Error.Conflict("Workforce.BranchAssignment.NotEditable", "errors.workforce.branchAssignment.notEditable");
    public static readonly Error InvalidStatusTransition = Error.Conflict("Workforce.BranchAssignment.InvalidStatusTransition", "errors.workforce.branchAssignment.invalidStatusTransition");
    public static readonly Error EmployeeEnded = Error.Conflict("Workforce.BranchAssignment.EmployeeEnded", "errors.workforce.branchAssignment.employeeEnded");
    public static readonly Error JobPositionDependsOnAssignment = Error.Conflict("Workforce.BranchAssignment.JobPositionDependsOnAssignment", "errors.workforce.branchAssignment.jobPositionDependsOnAssignment");
}
