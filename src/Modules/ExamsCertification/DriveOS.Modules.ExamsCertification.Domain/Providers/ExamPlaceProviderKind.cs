namespace DriveOS.Modules.ExamsCertification.Domain.Providers;

/// <summary>
/// Integration families supported by BC-11. The domain never depends directly on a national platform name.
/// </summary>
public enum ExamPlaceProviderKind
{
    Manual = 1,
    FileImport = 2,
    OfficialApi = 3,
    AuthorizedPartnerApi = 4,
    BrowserAgent = 5
}
