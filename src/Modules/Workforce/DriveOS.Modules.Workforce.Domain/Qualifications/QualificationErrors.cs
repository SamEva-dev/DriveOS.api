using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Domain.Qualifications;
public static class QualificationErrors
{
    public static readonly Error InvalidOwner = Error.Validation("Workforce.Qualification.InvalidOwner", "errors.workforce.qualification.invalidOwner");
    public static readonly Error InvalidQualification = Error.Validation("Workforce.Qualification.Invalid", "errors.workforce.qualification.invalid");
    public static readonly Error InvalidAuthorization = Error.Validation("Workforce.InstructorAuthorization.Invalid", "errors.workforce.instructorAuthorization.invalid");
    public static readonly Error InvalidValidityPeriod = Error.Validation("Workforce.Qualification.InvalidValidityPeriod", "errors.workforce.qualification.invalidValidityPeriod");
    public static readonly Error VerificationMethodRequired = Error.Validation("Workforce.Qualification.VerificationMethodRequired", "errors.workforce.qualification.verificationMethodRequired");
    public static readonly Error DecisionReasonRequired = Error.Validation("Workforce.Qualification.DecisionReasonRequired", "errors.workforce.qualification.decisionReasonRequired");
    public static readonly Error NotCurrent = Error.Conflict("Workforce.Qualification.NotCurrent", "errors.workforce.qualification.notCurrent");
    public static readonly Error NotFound = Error.NotFound("Workforce.Qualification.NotFound", "errors.workforce.qualification.notFound");
    public static readonly Error InstructorFunctionRequired = Error.Conflict("Workforce.InstructorAuthorization.InstructorFunctionRequired", "errors.workforce.instructorAuthorization.instructorFunctionRequired");
    public static readonly Error EmployeeNotEligible = Error.Conflict("Workforce.InstructorAuthorization.EmployeeNotEligible", "errors.workforce.instructorAuthorization.employeeNotEligible");
}
