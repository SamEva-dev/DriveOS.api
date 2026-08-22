namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.File;

public enum ExamRegistrationFileStatus
{
    Incomplete = 1,
    Ready = 2,
    Submitted = 3,
    OfficiallyAccepted = 4,
    OfficiallyRejected = 5,
    CorrectionRequested = 6,
    Cancelled = 7
}

public enum ExamRegistrationRequirementStatus
{
    Missing = 1,
    Pending = 2,
    Compliant = 3,
    Warning = 4,
    Blocked = 5,
    NotApplicable = 6
}

public static class ExamRegistrationRequirementCodes
{
    public const string IdentityVerified = "IdentityVerified";
    public const string OfficialDocument = "OfficialDocument";
    public const string Photograph = "Photograph";
    public const string PedagogicalOpinion = "PedagogicalOpinion";
    public const string RequiredTraining = "RequiredTraining";
    public const string CandidateReference = "CandidateReference";
    public const string RegulatoryTrainingRecord = "RegulatoryTrainingRecord";
}
