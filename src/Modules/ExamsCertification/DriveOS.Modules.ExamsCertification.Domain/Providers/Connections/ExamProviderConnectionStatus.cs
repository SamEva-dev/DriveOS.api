namespace DriveOS.Modules.ExamsCertification.Domain.Providers.Connections;

public enum ExamProviderConnectionStatus
{
    Draft = 1,
    PendingAuthorization = 2,
    Active = 3,
    Degraded = 4,
    Suspended = 5,
    Revoked = 6
}
