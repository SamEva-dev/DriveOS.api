using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Providers.Connections;

public static class ExamProviderConnectionErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Exams.ProviderConnection.InvalidIdentifier", "errors.exams.providerConnection.invalidIdentifier");
    public static readonly Error InvalidOrganization = Error.Validation("Exams.ProviderConnection.InvalidOrganization", "errors.exams.providerConnection.invalidOrganization");
    public static readonly Error InvalidProvider = Error.Validation("Exams.ProviderConnection.InvalidProvider", "errors.exams.providerConnection.invalidProvider");
    public static readonly Error InvalidCountry = Error.Validation("Exams.ProviderConnection.InvalidCountry", "errors.exams.providerConnection.invalidCountry");
    public static readonly Error InvalidEndpoint = Error.Validation("Exams.ProviderConnection.InvalidEndpoint", "errors.exams.providerConnection.invalidEndpoint");
    public static readonly Error NotFound = Error.NotFound("Exams.ProviderConnection.NotFound", "errors.exams.providerConnection.notFound");
    public static readonly Error Revoked = Error.Conflict("Exams.ProviderConnection.Revoked", "errors.exams.providerConnection.revoked");
}
