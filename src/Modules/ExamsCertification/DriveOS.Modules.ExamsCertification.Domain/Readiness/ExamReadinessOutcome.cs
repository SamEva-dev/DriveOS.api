namespace DriveOS.Modules.ExamsCertification.Domain.Readiness;

public enum ExamReadinessOutcome
{
    Ready = 1,
    ReadyWithConditions = 2,
    NotReady = 3,
    AdditionalTrainingRequired = 4,
    AdministrativeBlock = 5,
    FinancialBlock = 6,
    RegulatoryBlock = 7
}
