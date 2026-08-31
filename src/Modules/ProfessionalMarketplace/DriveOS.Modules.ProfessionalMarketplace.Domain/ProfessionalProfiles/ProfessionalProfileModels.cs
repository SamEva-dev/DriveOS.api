namespace DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;

public enum ProfessionalType
{
    DrivingInstructor = 0,
    InstructorTrainer = 1,
    AdministrativeContractor = 2,
    ComplianceConsultant = 3,
    Other = 99
}

public enum ProfessionalEngagementType
{
    HourlyService = 0,
    HalfDay = 1,
    FullDay = 2,
    FixedMission = 3,
    RecurringMission = 4,
    Replacement = 5,
    Negotiable = 99
}

public enum MarketplaceVisibility
{
    Private = 0,
    VerifiedOrganizationsOnly = 1,
    MarketplaceMembers = 2,
    Public = 3
}

public enum MarketplaceVerificationBadge
{
    None = 0,
    Pending = 1,
    Verified = 2,
    Restricted = 3
}
