using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Assignments;

public static class ExamResourceAssignmentErrors
{
    public static readonly Error NotFound = Error.NotFound("Exams.ResourceAssignment.NotFound", "errors.exams.resourceAssignment.notFound");
    public static readonly Error InvalidIdentifier = Error.Validation("Exams.ResourceAssignment.InvalidIdentifier", "errors.exams.resourceAssignment.invalidIdentifier");
    public static readonly Error OperationalPlanRequired = Error.Conflict("Exams.ResourceAssignment.OperationalPlanRequired", "errors.exams.resourceAssignment.operationalPlanRequired");
    public static readonly Error OperationalPlanNotReady = Error.Conflict("Exams.ResourceAssignment.OperationalPlanNotReady", "errors.exams.resourceAssignment.operationalPlanNotReady");
    public static readonly Error InstructorRequired = Error.Validation("Exams.ResourceAssignment.InstructorRequired", "errors.exams.resourceAssignment.instructorRequired");
    public static readonly Error VehicleRequired = Error.Validation("Exams.ResourceAssignment.VehicleRequired", "errors.exams.resourceAssignment.vehicleRequired");
    public static readonly Error InstructorNotEligible = Error.Conflict("Exams.ResourceAssignment.InstructorNotEligible", "errors.exams.resourceAssignment.instructorNotEligible");
    public static readonly Error VehicleNotEligible = Error.Conflict("Exams.ResourceAssignment.VehicleNotEligible", "errors.exams.resourceAssignment.vehicleNotEligible");
    public static readonly Error OperationConflict = Error.Conflict("Exams.ResourceAssignment.OperationConflict", "errors.exams.resourceAssignment.operationConflict");
    public static readonly Error SchedulingFailed = Error.Conflict("Exams.ResourceAssignment.SchedulingFailed", "errors.exams.resourceAssignment.schedulingFailed");
}
