using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;

public static class ProfessionalMissionErrors
{
    public static readonly Error NotFound=Error.NotFound("ProfessionalMarketplace.Missions.NotFound","errors.professionalMarketplace.missions.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("ProfessionalMarketplace.Missions.InvalidIdentifier","errors.professionalMarketplace.missions.invalidIdentifier");
    public static readonly Error ActiveEngagementRequired=Error.Conflict("ProfessionalMarketplace.Missions.ActiveEngagementRequired","errors.professionalMarketplace.missions.activeEngagementRequired");
    public static readonly Error InvalidContent=Error.Validation("ProfessionalMarketplace.Missions.InvalidContent","errors.professionalMarketplace.missions.invalidContent");
    public static readonly Error OutsideEngagementPeriod=Error.Validation("ProfessionalMarketplace.Missions.OutsideEngagementPeriod","errors.professionalMarketplace.missions.outsideEngagementPeriod");
    public static readonly Error InvalidTeachingCategories=Error.Validation("ProfessionalMarketplace.Missions.InvalidTeachingCategories","errors.professionalMarketplace.missions.invalidTeachingCategories");
    public static readonly Error InvalidEstimatedWorkload=Error.Validation("ProfessionalMarketplace.Missions.InvalidEstimatedWorkload","errors.professionalMarketplace.missions.invalidEstimatedWorkload");
    public static readonly Error BranchMismatch=Error.Conflict("ProfessionalMarketplace.Missions.BranchMismatch","errors.professionalMarketplace.missions.branchMismatch");
    public static readonly Error InvalidTimeWindows=Error.Validation("ProfessionalMarketplace.Missions.InvalidTimeWindows","errors.professionalMarketplace.missions.invalidTimeWindows");
    public static readonly Error InvalidTransition=Error.Conflict("ProfessionalMarketplace.Missions.InvalidTransition","errors.professionalMarketplace.missions.invalidTransition");
    public static readonly Error OutsideMissionPeriod=Error.Conflict("ProfessionalMarketplace.Missions.OutsideMissionPeriod","errors.professionalMarketplace.missions.outsideMissionPeriod");
    public static readonly Error StatusReasonRequired=Error.Validation("ProfessionalMarketplace.Missions.StatusReasonRequired","errors.professionalMarketplace.missions.statusReasonRequired");
    public static readonly Error MissionNotEndedYet=Error.Conflict("ProfessionalMarketplace.Missions.MissionNotEndedYet","errors.professionalMarketplace.missions.missionNotEndedYet");
}
