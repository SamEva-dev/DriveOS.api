namespace DriveOS.Modules.ExamsCertification.Domain.Results.Failure;

public enum ExamFailureRecommendationCode
{
    TargetedSessions = 1,
    MockExam = 2,
    PedagogicalReview = 3,
    RemediationPlan = 4,
    ChangeTrainingRhythm = 5,
    SecondOpinion = 6,
    ResumeAfterDelay = 7,
    ReestimateTrainingVolume = 8,
    Other = 99
}
