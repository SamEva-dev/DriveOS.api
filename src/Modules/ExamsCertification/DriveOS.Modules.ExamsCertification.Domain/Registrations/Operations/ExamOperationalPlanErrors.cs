using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Operations;

public static class ExamOperationalPlanErrors
{
    public static readonly Error NotFound = Error.NotFound("Exams.OperationalPlan.NotFound", "errors.exams.operationalPlan.notFound");
    public static readonly Error ConvocationRequired = Error.Conflict("Exams.OperationalPlan.ConvocationRequired", "errors.exams.operationalPlan.convocationRequired");
    public static readonly Error InvalidIdentifier = Error.Validation("Exams.OperationalPlan.InvalidIdentifier", "errors.exams.operationalPlan.invalidIdentifier");
    public static readonly Error InvalidWindow = Error.Validation("Exams.OperationalPlan.InvalidWindow", "errors.exams.operationalPlan.invalidWindow");
    public static readonly Error InvalidMeetingTime = Error.Validation("Exams.OperationalPlan.InvalidMeetingTime", "errors.exams.operationalPlan.invalidMeetingTime");
    public static readonly Error InvalidBuffer = Error.Validation("Exams.OperationalPlan.InvalidBuffer", "errors.exams.operationalPlan.invalidBuffer");
    public static readonly Error ConvocationVersionObsolete = Error.Conflict("Exams.OperationalPlan.ConvocationVersionObsolete", "errors.exams.operationalPlan.convocationVersionObsolete");
}
