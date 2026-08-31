namespace DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;

public interface IProfessionalMarketplaceUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
