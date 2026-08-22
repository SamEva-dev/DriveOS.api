namespace DriveOS.Modules.ExamsCertification.Domain.Providers;

[Flags]
public enum ExamPlaceProviderCapability
{
    None = 0,
    ReadAvailablePlaces = 1 << 0,
    ReadAssignedPlaces = 1 << 1,
    WatchAvailability = 1 << 2,
    ReservePlace = 1 << 3,
    ReleasePlace = 1 << 4,
    SubmitRegistration = 1 << 5,
    ReadRegistrationStatus = 1 << 6,
    ReadResults = 1 << 7
}
