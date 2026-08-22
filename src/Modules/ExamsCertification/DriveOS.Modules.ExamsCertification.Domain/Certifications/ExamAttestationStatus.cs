namespace DriveOS.Modules.ExamsCertification.Domain.Certifications;

public enum ExamAttestationStatus
{
    Generated = 1,
    Signed = 2,
    Delivered = 3,
    Revoked = 4,
    Expired = 5,
    Superseded = 6
}

public enum ExamAttestationDeliveryChannel
{
    Portal = 1,
    Email = 2,
    Printed = 3,
    InPerson = 4,
    ExternalDelivery = 5,
    Other = 99
}
