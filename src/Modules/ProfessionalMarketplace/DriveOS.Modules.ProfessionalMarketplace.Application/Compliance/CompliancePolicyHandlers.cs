using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Compliance;

public sealed class CreateComplianceCriticalityPolicyCommandHandler(
    IProfessionalCompliancePolicyRepository policies,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<CreateComplianceCriticalityPolicyCommand,ProfessionalCompliancePolicyId>
{
    public async Task<Result<ProfessionalCompliancePolicyId>> Handle(
        CreateComplianceCriticalityPolicyCommand c,CancellationToken ct)
    {
        if(await policies.ExistsVersionAsync(c.CountryCode,c.RequirementCode,c.Version,ct))
            return Result.Failure<ProfessionalCompliancePolicyId>(ProfessionalCompliancePolicyErrors.DuplicatePolicy);

        var created=ProfessionalComplianceCriticalityPolicy.Create(
            c.Id,c.CountryCode,c.RequirementCode,c.Criticality,c.Action,c.GracePeriodDays,
            c.EffectiveFrom,c.EffectiveTo,c.Version,clock.UtcNow,c.ActorUserId);

        if(created.IsFailure)return Result.Failure<ProfessionalCompliancePolicyId>(created.Error);

        policies.Add(created.Value);
        await uow.CommitAsync(ct);
        return Result.Success(created.Value.Id);
    }
}

public sealed class RetireComplianceCriticalityPolicyCommandHandler(
    IProfessionalCompliancePolicyRepository policies,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<RetireComplianceCriticalityPolicyCommand>
{
    public async Task<Result> Handle(RetireComplianceCriticalityPolicyCommand c,CancellationToken ct)
    {
        var policy=await policies.GetAsync(c.Id,true,ct);
        if(policy is null)return Result.Failure(ProfessionalCompliancePolicyErrors.NotFound);

        Result retired=policy.Retire(clock.UtcNow,c.ActorUserId);
        if(retired.IsFailure)return retired;

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class CreateProfessionalComplianceWaiverCommandHandler(
    IProfessionalComplianceWaiverRepository waivers,
    IProfessionalProfileRepository profiles,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<CreateProfessionalComplianceWaiverCommand,ProfessionalComplianceWaiverId>
{
    public async Task<Result<ProfessionalComplianceWaiverId>> Handle(
        CreateProfessionalComplianceWaiverCommand c,CancellationToken ct)
    {
        ProfessionalProfile? profile=await profiles.GetByIdAsync(c.ProfessionalProfileId,ct);
        if(profile is null)return Result.Failure<ProfessionalComplianceWaiverId>(ProfessionalProfileErrors.NotFound);
        if(string.IsNullOrWhiteSpace(profile.BillingCountryCode))
            return Result.Failure<ProfessionalComplianceWaiverId>(ProfessionalComplianceWaiverErrors.InvalidWaiver);

        if(await waivers.ExistsOverlappingAsync(
            c.ProfessionalProfileId,c.RequirementCode,c.ValidFrom,c.ValidUntil,ct))
            return Result.Failure<ProfessionalComplianceWaiverId>(ProfessionalComplianceWaiverErrors.DuplicateWaiver);

        var created=ProfessionalComplianceWaiver.Create(
            c.Id,c.ProfessionalProfileId,c.RequirementCode,profile.BillingCountryCode,
            c.ValidFrom,c.ValidUntil,c.Reason,clock.UtcNow,c.ActorUserId);

        if(created.IsFailure)return Result.Failure<ProfessionalComplianceWaiverId>(created.Error);

        waivers.Add(created.Value);
        await uow.CommitAsync(ct);
        return Result.Success(created.Value.Id);
    }
}

public sealed class RevokeProfessionalComplianceWaiverCommandHandler(
    IProfessionalComplianceWaiverRepository waivers,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock):ICommandHandler<RevokeProfessionalComplianceWaiverCommand>
{
    public async Task<Result> Handle(RevokeProfessionalComplianceWaiverCommand c,CancellationToken ct)
    {
        var waiver=await waivers.GetAsync(c.Id,true,ct);
        if(waiver is null||waiver.ProfessionalProfileId!=c.ProfessionalProfileId)
            return Result.Failure(ProfessionalComplianceWaiverErrors.NotFound);

        Result revoked=waiver.Revoke(c.Reason,clock.UtcNow,c.ActorUserId);
        if(revoked.IsFailure)return revoked;

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class GetComplianceCriticalityPoliciesQueryHandler(
    IProfessionalCompliancePolicyRepository policies)
    :IQueryHandler<GetComplianceCriticalityPoliciesQuery,IReadOnlyList<ComplianceCriticalityPolicyResponse>>
{
    public async Task<Result<IReadOnlyList<ComplianceCriticalityPolicyResponse>>> Handle(
        GetComplianceCriticalityPoliciesQuery q,CancellationToken ct)
    {
        var items=await policies.ListAsync(q.CountryCode,ct);
        return Result.Success<IReadOnlyList<ComplianceCriticalityPolicyResponse>>(
            items.Select(x=>new ComplianceCriticalityPolicyResponse(
                x.Id.Value,x.CountryCode,x.RequirementCode,x.Criticality.ToString(),x.Action.ToString(),
                x.GracePeriodDays,x.EffectiveFrom,x.EffectiveTo,x.Version,x.Status.ToString())).ToArray());
    }
}

public sealed class GetProfessionalComplianceWaiversQueryHandler(
    IProfessionalComplianceWaiverRepository waivers)
    :IQueryHandler<GetProfessionalComplianceWaiversQuery,IReadOnlyList<ProfessionalComplianceWaiverResponse>>
{
    public async Task<Result<IReadOnlyList<ProfessionalComplianceWaiverResponse>>> Handle(
        GetProfessionalComplianceWaiversQuery q,CancellationToken ct)
    {
        var items=await waivers.ListByProfileAsync(q.ProfessionalProfileId,ct);
        return Result.Success<IReadOnlyList<ProfessionalComplianceWaiverResponse>>(
            items.Select(x=>new ProfessionalComplianceWaiverResponse(
                x.Id.Value,x.ProfessionalProfileId.Value,x.RequirementCode,x.CountryCode,
                x.ValidFrom,x.ValidUntil,x.Reason,x.Status.ToString(),x.ApprovedByUserId.Value)).ToArray());
    }
}
