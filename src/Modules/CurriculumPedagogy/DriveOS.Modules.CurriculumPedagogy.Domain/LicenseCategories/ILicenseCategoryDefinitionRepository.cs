using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.CurriculumPedagogy.Domain.LicenseCategories;
public interface ILicenseCategoryDefinitionRepository
{
    Task<LicenseCategoryDefinition?> GetByIdAsync(LicenseCategoryDefinitionId id,CancellationToken cancellationToken=default);
    Task<LicenseCategoryDefinition?> GetByIdForUpdateAsync(LicenseCategoryDefinitionId id,CancellationToken cancellationToken=default);
    Task<LicenseCategoryDefinition?> GetActiveByScopeAsync(OrganizationId organizationId,string countryCode,string licenseCategoryCode,CancellationToken cancellationToken=default);
    Task<bool> ExistsAsync(OrganizationId organizationId,string countryCode,string licenseCategoryCode,CancellationToken cancellationToken=default);
    Task AddAsync(LicenseCategoryDefinition definition,CancellationToken cancellationToken=default);
}
