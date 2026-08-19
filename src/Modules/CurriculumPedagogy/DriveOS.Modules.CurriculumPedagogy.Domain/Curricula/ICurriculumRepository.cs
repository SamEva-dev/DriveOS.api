using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula;
public interface ICurriculumRepository
{
    Task<Curriculum?> GetByIdAsync(CurriculumId id,CancellationToken cancellationToken=default);
    Task<Curriculum?> GetByIdForUpdateAsync(CurriculumId id,CancellationToken cancellationToken=default);
    Task<bool> ExistsByCodeAsync(OrganizationId organizationId,string code,CancellationToken cancellationToken=default);
    Task AddAsync(Curriculum curriculum,CancellationToken cancellationToken=default);
}
