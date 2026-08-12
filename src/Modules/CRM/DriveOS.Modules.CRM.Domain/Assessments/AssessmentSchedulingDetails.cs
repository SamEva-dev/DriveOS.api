namespace DriveOS.Modules.CRM.Domain.Assessments;

public enum AssessmentType
{
    TheoryAssessment = 0,
    PracticalAssessment = 1,
    MixedAssessment = 2,
    SimulatorAssessment = 3,
    RemoteAssessment = 4,
    DocumentBasedAssessment = 5
}

public enum AssessmentDeliveryMode
{
    InPerson = 0,
    Remote = 1,
    Hybrid = 2,
    Mobile = 3,
    Partner = 4
}

public enum AssessmentLocationKind
{
    Branch = 0,
    MeetingPoint = 1,
    Simulator = 2,
    LeadAddress = 3,
    PartnerCenter = 4,
    VideoConference = 5
}
