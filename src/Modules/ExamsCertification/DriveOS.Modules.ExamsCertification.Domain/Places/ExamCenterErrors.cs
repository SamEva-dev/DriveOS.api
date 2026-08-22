using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Places;

public static class ExamCenterErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Exams.Center.InvalidIdentifier", "errors.exams.center.invalidIdentifier");
    public static readonly Error InvalidOrganization = Error.Validation("Exams.Center.InvalidOrganization", "errors.exams.center.invalidOrganization");
    public static readonly Error InvalidName = Error.Validation("Exams.Center.InvalidName", "errors.exams.center.invalidName");
    public static readonly Error InvalidCountry = Error.Validation("Exams.Center.InvalidCountry", "errors.exams.center.invalidCountry");
    public static readonly Error InvalidTimeZone = Error.Validation("Exams.Center.InvalidTimeZone", "errors.exams.center.invalidTimeZone");
    public static readonly Error InvalidStatus = Error.Validation("Exams.Center.InvalidStatus", "errors.exams.center.invalidStatus");
}
