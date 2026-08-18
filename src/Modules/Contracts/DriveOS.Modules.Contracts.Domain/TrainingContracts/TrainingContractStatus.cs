namespace DriveOS.Modules.Contracts.Domain.TrainingContracts;

public enum TrainingContractStatus
{
    Draft = 0,
    Generated = 1,
    SentForSignature = 2,
    PartiallySigned = 3,
    Signed = 4,
    Active = 5,
    Amended = 6,
    Suspended = 7,
    Terminated = 8,
    Completed = 9,
    Cancelled = 10,
    Expired = 11
}
