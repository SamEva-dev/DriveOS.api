using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.Transfers;

public static class InternalTransferErrors
{
    public static readonly Error InvalidOwner = Error.Validation(
        "Students.InternalTransfer.InvalidOwner",
        "errors.students.internalTransfer.invalidOwner"
    );
    public static readonly Error InvalidRequest = Error.Validation(
        "Students.InternalTransfer.InvalidRequest",
        "errors.students.internalTransfer.invalidRequest"
    );
    public static readonly Error ReasonRequired = Error.Validation(
        "Students.InternalTransfer.ReasonRequired",
        "errors.students.internalTransfer.reasonRequired"
    );
    public static readonly Error EffectiveDateRequired = Error.Validation(
        "Students.InternalTransfer.EffectiveDateRequired",
        "errors.students.internalTransfer.effectiveDateRequired"
    );
    public static readonly Error SameBranch = Error.Conflict(
        "Students.InternalTransfer.SameBranch",
        "errors.students.internalTransfer.sameBranch"
    );
    public static readonly Error ActiveTransferExists = Error.Conflict(
        "Students.InternalTransfer.ActiveTransferExists",
        "errors.students.internalTransfer.activeTransferExists"
    );
    public static readonly Error AnalysisNotFound = Error.NotFound(
        "Students.InternalTransfer.AnalysisNotFound",
        "errors.students.internalTransfer.analysisNotFound"
    );
    public static readonly Error AnalysisExpired = Error.Conflict(
        "Students.InternalTransfer.AnalysisExpired",
        "errors.students.internalTransfer.analysisExpired"
    );
    public static readonly Error BlockingImpact = Error.Conflict(
        "Students.InternalTransfer.BlockingImpact",
        "errors.students.internalTransfer.blockingImpact"
    );
    public static readonly Error AlreadyValidated = Error.Conflict(
        "Students.InternalTransfer.AlreadyValidated",
        "errors.students.internalTransfer.alreadyValidated"
    );
}
