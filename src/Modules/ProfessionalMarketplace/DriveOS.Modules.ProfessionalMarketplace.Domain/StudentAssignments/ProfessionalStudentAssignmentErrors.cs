using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.StudentAssignments;

public static class ProfessionalStudentAssignmentErrors
{
    public static readonly Error NotFound=Error.NotFound(
        "ProfessionalMarketplace.StudentAssignments.NotFound",
        "errors.professionalMarketplace.studentAssignments.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation(
        "ProfessionalMarketplace.StudentAssignments.InvalidIdentifier",
        "errors.professionalMarketplace.studentAssignments.invalidIdentifier");
    public static readonly Error OutsideMissionPeriod=Error.Validation(
        "ProfessionalMarketplace.StudentAssignments.OutsideMissionPeriod",
        "errors.professionalMarketplace.studentAssignments.outsideMissionPeriod");
    public static readonly Error InvalidScope=Error.Validation(
        "ProfessionalMarketplace.StudentAssignments.InvalidScope",
        "errors.professionalMarketplace.studentAssignments.invalidScope");
    public static readonly Error DuplicateAssignment=Error.Conflict(
        "ProfessionalMarketplace.StudentAssignments.DuplicateAssignment",
        "errors.professionalMarketplace.studentAssignments.duplicateAssignment");
    public static readonly Error InvalidTransition=Error.Conflict(
        "ProfessionalMarketplace.StudentAssignments.InvalidTransition",
        "errors.professionalMarketplace.studentAssignments.invalidTransition");
    public static readonly Error RevocationReasonRequired=Error.Validation(
        "ProfessionalMarketplace.StudentAssignments.RevocationReasonRequired",
        "errors.professionalMarketplace.studentAssignments.revocationReasonRequired");
    public static readonly Error StudentNotFound=Error.NotFound(
        "ProfessionalMarketplace.StudentAssignments.StudentNotFound",
        "errors.professionalMarketplace.studentAssignments.studentNotFound");
    public static readonly Error ActiveMissionRequired=Error.Conflict(
        "ProfessionalMarketplace.StudentAssignments.ActiveMissionRequired",
        "errors.professionalMarketplace.studentAssignments.activeMissionRequired");
    public static readonly Error AssignmentReasonRequired=Error.Validation("ProfessionalMarketplace.StudentAssignments.AssignmentReasonRequired","errors.professionalMarketplace.studentAssignments.assignmentReasonRequired");
}
