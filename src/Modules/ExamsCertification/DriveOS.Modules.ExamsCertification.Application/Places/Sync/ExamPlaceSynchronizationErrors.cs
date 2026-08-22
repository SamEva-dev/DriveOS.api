using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Places.Sync;

public static class ExamPlaceSynchronizationErrors
{
    public static readonly Error InvalidPeriod = Error.Validation(
        "Exams.PlaceSync.InvalidPeriod",
        "errors.exams.placeSync.invalidPeriod");

    public static readonly Error InvalidProvider = Error.Validation(
        "Exams.PlaceSync.InvalidProvider",
        "errors.exams.placeSync.invalidProvider");

    public static readonly Error ProviderNotFound = Error.NotFound(
        "Exams.PlaceSync.ProviderNotFound",
        "errors.exams.placeSync.providerNotFound");

    public static readonly Error ProviderDoesNotExposeAvailability = Error.Validation(
        "Exams.PlaceSync.ProviderDoesNotExposeAvailability",
        "errors.exams.placeSync.providerDoesNotExposeAvailability");

    public static Error ProviderUnavailable(string providerCode) => Error.Failure(
        "Exams.PlaceSync.ProviderUnavailable",
        "errors.exams.placeSync.providerUnavailable",
        new Dictionary<string, object?> { ["providerCode"] = providerCode });

    public static readonly Error EmptyImport = Error.Validation(
        "Exams.PlaceSync.EmptyImport",
        "errors.exams.placeSync.emptyImport");

    public static readonly Error InvalidExternalPlace = Error.Validation(
        "Exams.PlaceSync.InvalidExternalPlace",
        "errors.exams.placeSync.invalidExternalPlace");
}
