namespace DriveOS.Modules.ExamsCertification.Domain.Results;

/// <summary>Origin of a result. ProviderCode carries the concrete authority/integration without coupling the domain to a national system.</summary>
public enum ExamResultSourceKind
{
    Manual = 1,
    FileImport = 2,
    OfficialApi = 3,
    AuthorizedPartnerApi = 4,
    ExternalProvider = 5
}
