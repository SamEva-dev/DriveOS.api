using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.AccessGrants;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.AccessGrants;

public sealed class PrepareProfessionalEngagementAccessCommandHandler(
    IExternalAccessGrantRepository grants,
    IProfessionalEngagementRepository engagements,
    IProfessionalProfileRepository profiles,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock) : ICommandHandler<PrepareProfessionalEngagementAccessCommand,ExternalAccessPreparationResult>
{
    public async Task<Result<ExternalAccessPreparationResult>> Handle(PrepareProfessionalEngagementAccessCommand c,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(c.EngagementId,true,ct);
        if(engagement is null||engagement.OrganizationId!=c.OrganizationId)
            return Result.Failure<ExternalAccessPreparationResult>(ProfessionalEngagementErrors.NotFound);

        if(engagement.Status is ProfessionalEngagementStatus.Ended or ProfessionalEngagementStatus.Terminated)
            return Result.Failure<ExternalAccessPreparationResult>(ExternalAccessGrantErrors.ActiveEngagementRequired);

        ProfessionalProfile? profile=await profiles.GetByIdAsync(engagement.ProfessionalProfileId,ct);
        if(profile is null||(!profile.UserId.HasValue || profile.UserId.Value.IsEmpty))
            return Result.Success(new ExternalAccessPreparationResult(false,null,"professional-marketplace.access-grants.professional-user-required"));

        const string baselinePermission="READ";
        bool exists=await grants.ExistsActiveAsync(
            engagement.Id,
            ExternalAccessResourceType.Engagement,
            engagement.Id.Value,
            baselinePermission,
            ct);

        Guid? grantId=null;
        if(!exists)
        {
            ExternalAccessGrantId id=new(Guid.NewGuid());
            Result<ExternalAccessGrant> created=ExternalAccessGrant.Create(
                id,
                engagement.Id,
                engagement.ProfessionalProfileId,
                engagement.OrganizationId,
                engagement.BranchId,
                ExternalAccessResourceType.Engagement,
                engagement.Id.Value,
                baselinePermission,
                engagement.StartsOn,
                engagement.EndsOn,
                engagement.StartsOn,
                engagement.EndsOn,
                clock.UtcNow,
                c.ActorUserId);

            if(created.IsFailure)
                return Result.Failure<ExternalAccessPreparationResult>(created.Error);

            grants.Add(created.Value);
            grantId=id.Value;
        }

        Result marked=engagement.MarkPreparation(
            EngagementPreparationStep.Access,
            true,
            clock.UtcNow,
            c.ActorUserId);

        if(marked.IsFailure)
            return Result.Failure<ExternalAccessPreparationResult>(marked.Error);

        await uow.CommitAsync(ct);
        return Result.Success(new ExternalAccessPreparationResult(true,grantId,null));
    }
}

public sealed class CreateExternalAccessGrantCommandHandler(
    IExternalAccessGrantRepository grants,
    IProfessionalEngagementRepository engagements,
    IProfessionalMissionRepository missions,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock) : ICommandHandler<CreateExternalAccessGrantCommand,ExternalAccessGrantId>
{
    public async Task<Result<ExternalAccessGrantId>> Handle(CreateExternalAccessGrantCommand c,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(c.EngagementId,false,ct);
        if(engagement is null||engagement.OrganizationId!=c.OrganizationId)
            return Result.Failure<ExternalAccessGrantId>(ProfessionalEngagementErrors.NotFound);

        if(engagement.Status!=ProfessionalEngagementStatus.Active)
            return Result.Failure<ExternalAccessGrantId>(ExternalAccessGrantErrors.ActiveEngagementRequired);

        if(c.ResourceType==ExternalAccessResourceType.Mission)
        {
            ProfessionalMission? mission=await missions.GetAsync(new ProfessionalMissionId(c.ResourceId),false,ct);
            if(mission is null||mission.EngagementId!=engagement.Id||mission.OrganizationId!=c.OrganizationId)
                return Result.Failure<ExternalAccessGrantId>(ProfessionalMissionErrors.NotFound);
            if(c.StartDate<mission.StartsOn||c.EndDate>mission.EndsOn)
                return Result.Failure<ExternalAccessGrantId>(ExternalAccessGrantErrors.OutsideResourcePeriod);
        }

        string permission=(c.Permission??string.Empty).Trim().ToUpperInvariant();
        if(await grants.ExistsActiveAsync(c.EngagementId,c.ResourceType,c.ResourceId,permission,ct))
            return Result.Failure<ExternalAccessGrantId>(ExternalAccessGrantErrors.DuplicateGrant);

        Result<ExternalAccessGrant> created=ExternalAccessGrant.Create(
            c.Id,
            engagement.Id,
            engagement.ProfessionalProfileId,
            engagement.OrganizationId,
            engagement.BranchId,
            c.ResourceType,
            c.ResourceId,
            permission,
            c.StartDate,
            c.EndDate,
            engagement.StartsOn,
            engagement.EndsOn,
            clock.UtcNow,
            c.ActorUserId);

        if(created.IsFailure)return Result.Failure<ExternalAccessGrantId>(created.Error);

        grants.Add(created.Value);
        await uow.CommitAsync(ct);
        return Result.Success(created.Value.Id);
    }
}

public sealed class RevokeExternalAccessGrantCommandHandler(
    IExternalAccessGrantRepository grants,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock) : ICommandHandler<RevokeExternalAccessGrantCommand>
{
    public async Task<Result> Handle(RevokeExternalAccessGrantCommand c,CancellationToken ct)
    {
        ExternalAccessGrant? grant=await grants.GetAsync(c.Id,true,ct);
        if(grant is null||grant.OrganizationId!=c.OrganizationId)
            return Result.Failure(ExternalAccessGrantErrors.NotFound);

        Result result=grant.Revoke(c.Reason,clock.UtcNow,c.ActorUserId);
        if(result.IsFailure)return result;

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class CheckExternalProfessionalAccessQueryHandler(
    IExternalAccessGrantRepository grants) : IQueryHandler<CheckExternalProfessionalAccessQuery,bool>
{
    public async Task<Result<bool>> Handle(CheckExternalProfessionalAccessQuery q,CancellationToken ct)
    {
        bool allowed=await grants.HasEffectiveGrantAsync(
            q.ProfessionalProfileId,q.OrganizationId,q.ResourceType,q.ResourceId,q.Permission,q.Date,ct);
        return Result.Success(allowed);
    }
}


public sealed class ListExternalAccessGrantsQueryHandler(
    IExternalAccessGrantRepository grants,
    IProfessionalEngagementRepository engagements) : IQueryHandler<ListExternalAccessGrantsQuery,IReadOnlyList<ExternalAccessGrantReadModel>>
{
    public async Task<Result<IReadOnlyList<ExternalAccessGrantReadModel>>> Handle(ListExternalAccessGrantsQuery q,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(q.EngagementId,false,ct);
        if(engagement is null||engagement.OrganizationId!=q.OrganizationId)
            return Result.Failure<IReadOnlyList<ExternalAccessGrantReadModel>>(ProfessionalEngagementErrors.NotFound);

        IReadOnlyList<ExternalAccessGrant> rows=await grants.ListByEngagementAsync(q.EngagementId,ct);
        IReadOnlyList<ExternalAccessGrantReadModel> result=rows.Select(x=>new ExternalAccessGrantReadModel(
            x.Id.Value,
            x.EngagementId.Value,
            x.ProfessionalProfileId.Value,
            x.OrganizationId.Value,
            x.BranchId?.Value,
            x.ResourceType,
            x.ResourceId,
            x.Permission,
            x.StartDate,
            x.EndDate,
            x.Status,
            x.GrantedByUserId.Value,
            x.CreatedAtUtc,
            x.RevokedAtUtc,
            x.RevokedByUserId?.Value,
            x.RevocationReason,
            ResolveOrigin(x))).ToArray();

        return Result.Success(result);
    }

    private static string ResolveOrigin(ExternalAccessGrant grant) => grant.ResourceType switch
    {
        ExternalAccessResourceType.Engagement when grant.ResourceId==grant.EngagementId.Value => "ENGAGEMENT_PREPARATION",
        ExternalAccessResourceType.Student => "STUDENT_ASSIGNMENT",
        ExternalAccessResourceType.Mission => "MISSION_SCOPE",
        _ => "MANUAL"
    };
}
