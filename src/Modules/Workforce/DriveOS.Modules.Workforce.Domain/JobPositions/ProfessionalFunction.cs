namespace DriveOS.Modules.Workforce.Domain.JobPositions;

/// <summary>
/// Stable business classification used by Workforce workflows. It is not a security role and does not grant permissions.
/// </summary>
public enum ProfessionalFunction
{
    Instructor = 1,
    PedagogicalManagement = 2,
    BranchManagement = 3,
    Administration = 4,
    Finance = 5,
    Executive = 6,
    FleetManagement = 7,
    ExamCoordination = 8,
    Sales = 9,
    HumanResources = 10,
    Support = 11,
    Other = 99
}
