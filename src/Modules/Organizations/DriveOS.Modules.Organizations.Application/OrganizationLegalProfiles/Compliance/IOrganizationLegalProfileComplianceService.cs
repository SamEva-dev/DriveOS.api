using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;

public interface IOrganizationLegalProfileComplianceService
{
    OrganizationLegalProfileComplianceResult Validate(OrganizationLegalProfile profile);
}
