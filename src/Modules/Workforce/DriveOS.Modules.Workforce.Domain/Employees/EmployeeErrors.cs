using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Workforce.Domain.Employees;

public static class EmployeeErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Workforce.Employee.InvalidIdentifier", "errors.workforce.employee.invalidIdentifier");
    public static readonly Error InvalidEmployer = Error.Validation("Workforce.Employee.InvalidEmployer", "errors.workforce.employee.invalidEmployer");
    public static readonly Error PersonRequired = Error.Validation("Workforce.Employee.PersonRequired", "errors.workforce.employee.personRequired");
    public static readonly Error EmployeeNumberRequired = Error.Validation("Workforce.Employee.EmployeeNumberRequired", "errors.workforce.employee.employeeNumberRequired");
    public static readonly Error EmployeeNumberTooLong = Error.Validation("Workforce.Employee.EmployeeNumberTooLong", "errors.workforce.employee.employeeNumberTooLong");
    public static readonly Error InvalidEmploymentPeriod = Error.Validation("Workforce.Employee.InvalidEmploymentPeriod", "errors.workforce.employee.invalidEmploymentPeriod");
    public static readonly Error DuplicateEmployeeNumber = Error.Conflict("Workforce.Employee.DuplicateEmployeeNumber", "errors.workforce.employee.duplicateEmployeeNumber");
    public static readonly Error ExistingEmployment = Error.Conflict("Workforce.Employee.ExistingEmployment", "errors.workforce.employee.existingEmployment");
    public static readonly Error UserAlreadyLinked = Error.Conflict("Workforce.Employee.UserAlreadyLinked", "errors.workforce.employee.userAlreadyLinked");
    public static readonly Error NotFound = Error.NotFound("Workforce.Employee.NotFound", "errors.workforce.employee.notFound");
    public static readonly Error InvalidLifecycleTransition = Error.Conflict("Workforce.Employee.InvalidLifecycleTransition", "errors.workforce.employee.invalidLifecycleTransition");
    public static readonly Error LifecycleReasonRequired = Error.Validation("Workforce.Employee.LifecycleReasonRequired", "errors.workforce.employee.lifecycleReasonRequired");
    public static readonly Error EndedEmploymentImmutable = Error.Conflict("Workforce.Employee.EndedEmploymentImmutable", "errors.workforce.employee.endedEmploymentImmutable");
    public static readonly Error InvalidRehireUserSelection = Error.Validation("Workforce.Employee.InvalidRehireUserSelection", "errors.workforce.employee.invalidRehireUserSelection");
    public static readonly Error RehireRequiresEndedEmployment = Error.Conflict("Workforce.Employee.RehireRequiresEndedEmployment", "errors.workforce.employee.rehireRequiresEndedEmployment");
    public static readonly Error RehireMustStartAfterPreviousEmployment = Error.Validation("Workforce.Employee.RehireMustStartAfterPreviousEmployment", "errors.workforce.employee.rehireMustStartAfterPreviousEmployment");
    public static readonly Error RehireSourceMustBeLatestEmployment = Error.Conflict("Workforce.Employee.RehireSourceMustBeLatestEmployment", "errors.workforce.employee.rehireSourceMustBeLatestEmployment");
    public static readonly Error InvalidTerminationDate = Error.Validation("Workforce.Employee.InvalidTerminationDate", "errors.workforce.employee.invalidTerminationDate");
}
