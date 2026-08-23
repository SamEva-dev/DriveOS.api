using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Domain.EmploymentContracts;
public static class EmploymentContractErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Workforce.EmploymentContract.InvalidIdentifier","errors.workforce.employmentContract.invalidIdentifier");
    public static readonly Error InvalidPeriod = Error.Validation("Workforce.EmploymentContract.InvalidPeriod","errors.workforce.employmentContract.invalidPeriod");
    public static readonly Error InvalidWeeklyHours = Error.Validation("Workforce.EmploymentContract.InvalidWeeklyHours","errors.workforce.employmentContract.invalidWeeklyHours");
    public static readonly Error InvalidDocumentReference = Error.Validation("Workforce.EmploymentContract.InvalidDocumentReference","errors.workforce.employmentContract.invalidDocumentReference");
    public static readonly Error InvalidLifecycleTransition = Error.Conflict("Workforce.EmploymentContract.InvalidLifecycleTransition","errors.workforce.employmentContract.invalidLifecycleTransition");
    public static readonly Error ImmutableAfterSignatureFlow = Error.Conflict("Workforce.EmploymentContract.ImmutableAfterSignatureFlow","errors.workforce.employmentContract.immutableAfterSignatureFlow");
    public static readonly Error PeriodOverlap = Error.Conflict("Workforce.EmploymentContract.PeriodOverlap","errors.workforce.employmentContract.periodOverlap");
    public static readonly Error NotFound = Error.NotFound("Workforce.EmploymentContract.NotFound","errors.workforce.employmentContract.notFound");
    public static readonly Error EmployeeEnded = Error.Conflict("Workforce.EmploymentContract.EmployeeEnded","errors.workforce.employmentContract.employeeEnded");
    public static readonly Error ActivationBeforeStartDate = Error.Validation("Workforce.EmploymentContract.ActivationBeforeStartDate","errors.workforce.employmentContract.activationBeforeStartDate");
}
