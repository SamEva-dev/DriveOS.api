using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Workforce.Domain.LeavePolicies;
public static class LeavePolicyErrors
{
 public static readonly Error InvalidIdentifier=Error.Validation("Workforce.LeavePolicy.InvalidIdentifier","errors.workforce.leavePolicy.invalidIdentifier");
 public static readonly Error InvalidOrganization=Error.Validation("Workforce.LeavePolicy.InvalidOrganization","errors.workforce.leavePolicy.invalidOrganization");
 public static readonly Error InvalidCountryCode=Error.Validation("Workforce.LeavePolicy.InvalidCountryCode","errors.workforce.leavePolicy.invalidCountryCode");
 public static readonly Error CodeRequired=Error.Validation("Workforce.LeavePolicy.CodeRequired","errors.workforce.leavePolicy.codeRequired");
 public static readonly Error CodeTooLong=Error.Validation("Workforce.LeavePolicy.CodeTooLong","errors.workforce.leavePolicy.codeTooLong");
 public static readonly Error NameRequired=Error.Validation("Workforce.LeavePolicy.NameRequired","errors.workforce.leavePolicy.nameRequired");
 public static readonly Error NameTooLong=Error.Validation("Workforce.LeavePolicy.NameTooLong","errors.workforce.leavePolicy.nameTooLong");
 public static readonly Error InvalidRuleValue=Error.Validation("Workforce.LeavePolicy.InvalidRuleValue","errors.workforce.leavePolicy.invalidRuleValue");
 public static readonly Error DuplicateCode=Error.Conflict("Workforce.LeavePolicy.DuplicateCode","errors.workforce.leavePolicy.duplicateCode");
 public static readonly Error NotFound=Error.NotFound("Workforce.LeavePolicy.NotFound","errors.workforce.leavePolicy.notFound");
 public static readonly Error AlreadyInactive=Error.Conflict("Workforce.LeavePolicy.AlreadyInactive","errors.workforce.leavePolicy.alreadyInactive");
 public static readonly Error AlreadyActive=Error.Conflict("Workforce.LeavePolicy.AlreadyActive","errors.workforce.leavePolicy.alreadyActive");
}
