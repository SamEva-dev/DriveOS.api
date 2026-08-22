using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Places.Watch;

public static class ExamPlaceWatchErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Exams.PlaceWatch.InvalidIdentifier", "errors.exams.placeWatch.invalidIdentifier");
    public static readonly Error InvalidOrganization = Error.Validation("Exams.PlaceWatch.InvalidOrganization", "errors.exams.placeWatch.invalidOrganization");
    public static readonly Error InvalidProvider = Error.Validation("Exams.PlaceWatch.InvalidProvider", "errors.exams.placeWatch.invalidProvider");
    public static readonly Error InvalidCountry = Error.Validation("Exams.PlaceWatch.InvalidCountry", "errors.exams.placeWatch.invalidCountry");
    public static readonly Error InvalidPeriod = Error.Validation("Exams.PlaceWatch.InvalidPeriod", "errors.exams.placeWatch.invalidPeriod");
    public static readonly Error InvalidInterval = Error.Validation("Exams.PlaceWatch.InvalidInterval", "errors.exams.placeWatch.invalidInterval");
    public static readonly Error NotActive = Error.Conflict("Exams.PlaceWatch.NotActive", "errors.exams.placeWatch.notActive");
    public static readonly Error AlreadyActive = Error.Conflict("Exams.PlaceWatch.AlreadyActive", "errors.exams.placeWatch.alreadyActive");
    public static readonly Error Ended = Error.Conflict("Exams.PlaceWatch.Ended", "errors.exams.placeWatch.ended");
    public static readonly Error NotFound = Error.NotFound("Exams.PlaceWatch.NotFound", "errors.exams.placeWatch.notFound");
}
