namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

public enum TrainingSessionMarkerType
{
    CompetencyObserved = 1,
    Success = 2,
    Difficulty = 3,
    InstructorIntervention = 4,
    SafetyEvent = 5,
    QuestionToReview = 6,
    StudentComment = 7,
    RouteContext = 8,
    Incident = 9
}

public enum TrainingSessionMarkerSeverity
{
    Information = 1,
    Attention = 2,
    Important = 3,
    Critical = 4
}
