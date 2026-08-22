namespace DriveOS.Modules.ExamsCertification.Domain.Providers.Connections;

public enum ExamProviderAuthenticationMode
{
    None = 1,
    OAuth2AuthorizationCode = 2,
    OAuth2ClientCredentials = 3,
    ApiKey = 4,
    LocalAgent = 5,
    ExternalManaged = 6
}
