using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Places;

public static class ExamPlaceErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Exams.Place.InvalidIdentifier", "errors.exams.place.invalidIdentifier");
    public static readonly Error InvalidOrganization = Error.Validation("Exams.Place.InvalidOrganization", "errors.exams.place.invalidOrganization");
    public static readonly Error InvalidCenter = Error.Validation("Exams.Place.InvalidCenter", "errors.exams.place.invalidCenter");
    public static readonly Error InvalidPeriod = Error.Validation("Exams.Place.InvalidPeriod", "errors.exams.place.invalidPeriod");
    public static readonly Error InvalidCategory = Error.Validation("Exams.Place.InvalidCategory", "errors.exams.place.invalidCategory");
    public static readonly Error InvalidProvider = Error.Validation("Exams.Place.InvalidProvider", "errors.exams.place.invalidProvider");
    public static readonly Error NotAvailable = Error.Conflict("Exams.Place.NotAvailable", "errors.exams.place.notAvailable");
    public static readonly Error HoldExpired = Error.Conflict("Exams.Place.HoldExpired", "errors.exams.place.holdExpired");
    public static readonly Error HoldTokenMismatch = Error.Conflict("Exams.Place.HoldTokenMismatch", "errors.exams.place.holdTokenMismatch");
    public static readonly Error AlreadyAssigned = Error.Conflict("Exams.Place.AlreadyAssigned", "errors.exams.place.alreadyAssigned");
    public static readonly Error InvalidStudent = Error.Validation("Exams.Place.InvalidStudent", "errors.exams.place.invalidStudent");
    public static readonly Error InvalidRegistration = Error.Validation("Exams.Place.InvalidRegistration", "errors.exams.place.invalidRegistration");
}
