using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.ProfessionalProfiles;
public sealed class ChangeMarketplaceVisibilityCommandHandler(
    IProfessionalProfileRepository profiles,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<ChangeMarketplaceVisibilityCommand>
{
    public async Task<Result> Handle(ChangeMarketplaceVisibilityCommand c,CancellationToken ct)
    {
        var profile=await profiles.GetByIdForUpdateAsync(c.ProfileId,ct);
        if(profile is null)return Result.Failure(ProfessionalProfileErrors.NotFound);
        var r=profile.ChangeMarketplaceVisibility(c.Visibility,clock.UtcNow,c.ActorUserId);
        if(r.IsFailure)return r;
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
