using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ExamsCertification.Domain.Results.Success;
public static class ExamSuccessProcessErrors
{
    public static readonly Error NotFound = Error.NotFound("Exams.Success.NotFound", "errors.exams.success.notFound");
    public static readonly Error BlockingActionsRemain = Error.Conflict("Exams.Success.BlockingActionsRemain", "errors.exams.success.blockingActionsRemain");
    public static readonly Error AlreadyCompleted = Error.Conflict("Exams.Success.AlreadyCompleted", "errors.exams.success.alreadyCompleted");
    public static readonly Error Superseded = Error.Conflict("Exams.Success.Superseded", "errors.exams.success.superseded");
    public static readonly Error ActionNotFound = Error.NotFound("Exams.Success.ActionNotFound", "errors.exams.success.actionNotFound");
}
