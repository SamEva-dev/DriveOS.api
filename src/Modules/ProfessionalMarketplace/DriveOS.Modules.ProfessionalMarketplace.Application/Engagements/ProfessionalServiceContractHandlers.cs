using System.Text.Json;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;

public sealed class CreateProfessionalServiceContractCommandHandler(
    IProfessionalEngagementRepository engagements,
    IProfessionalProfileRepository profiles,
    IProfessionalServiceContractGateway contracts)
    :ICommandHandler<CreateProfessionalServiceContractCommand,ProfessionalServiceContractSnapshot>
{
    public async Task<Result<ProfessionalServiceContractSnapshot>> Handle(
        CreateProfessionalServiceContractCommand c,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(c.EngagementId,false,ct);
        if(engagement is null||engagement.OrganizationId!=c.OrganizationId)
            return Result.Failure<ProfessionalServiceContractSnapshot>(ProfessionalEngagementErrors.NotFound);

        ProfessionalProfile? profile=await profiles.GetByIdAsync(engagement.ProfessionalProfileId,ct);
        if(profile is null)
            return Result.Failure<ProfessionalServiceContractSnapshot>(ProfessionalProfileErrors.NotFound);

        Result<ProfessionalServiceContractSnapshot> result=await contracts.CreateAsync(new(
            c.Id,
            engagement.OrganizationId,
            engagement.Id,
            engagement.ProfessionalProfileId,
            profile.ProviderOrganizationId.Value,
            c.ContractNumber,
            c.ContractType,
            c.SignatureOrder,
            JsonSerializer.Serialize(engagement.TermsSnapshot),
            c.Signatories??[],
            c.ActorUserId),ct);

        return result;
    }
}

public sealed class GenerateProfessionalServiceContractCommandHandler(
    IProfessionalEngagementRepository engagements,
    IProfessionalServiceContractGateway contracts)
    :ICommandHandler<GenerateProfessionalServiceContractCommand,ProfessionalServiceContractSnapshot>
{
    public async Task<Result<ProfessionalServiceContractSnapshot>> Handle(
        GenerateProfessionalServiceContractCommand c,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(c.EngagementId,false,ct);
        if(engagement is null||engagement.OrganizationId!=c.OrganizationId)
            return Result.Failure<ProfessionalServiceContractSnapshot>(ProfessionalEngagementErrors.NotFound);

        return await contracts.GenerateAsync(
            engagement.Id,c.DocumentReference,c.DocumentSha256,c.ActorUserId,ct);
    }
}

public sealed class ReviseProfessionalServiceContractCommandHandler(
    IProfessionalEngagementRepository engagements,
    IProfessionalServiceContractGateway contracts,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock)
    :ICommandHandler<ReviseProfessionalServiceContractCommand,ProfessionalServiceContractSnapshot>
{
    public async Task<Result<ProfessionalServiceContractSnapshot>> Handle(
        ReviseProfessionalServiceContractCommand c,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(c.EngagementId,true,ct);
        if(engagement is null||engagement.OrganizationId!=c.OrganizationId)
            return Result.Failure<ProfessionalServiceContractSnapshot>(ProfessionalEngagementErrors.NotFound);

        Result<ProfessionalServiceContractSnapshot> revised=await contracts.CreateRevisionAsync(
            engagement.Id,c.DocumentReference,c.DocumentSha256,c.Reason,c.ActorUserId,ct);
        if(revised.IsFailure)return revised;

        Result unprepared=engagement.MarkPreparation(
            EngagementPreparationStep.Contract,false,clock.UtcNow,c.ActorUserId);
        if(unprepared.IsFailure)
            return Result.Failure<ProfessionalServiceContractSnapshot>(unprepared.Error);

        if(engagement.Status==ProfessionalEngagementStatus.Active)
        {
            Result suspended=engagement.Suspend("Contract revision requires new signatures",clock.UtcNow,c.ActorUserId);
            if(suspended.IsFailure)
                return Result.Failure<ProfessionalServiceContractSnapshot>(suspended.Error);
        }

        await uow.CommitAsync(ct);
        return revised;
    }
}

public sealed class SendProfessionalServiceContractForSignatureCommandHandler(
    IProfessionalEngagementRepository engagements,
    IProfessionalServiceContractGateway contracts)
    :ICommandHandler<SendProfessionalServiceContractForSignatureCommand,ProfessionalServiceContractSnapshot>
{
    public async Task<Result<ProfessionalServiceContractSnapshot>> Handle(
        SendProfessionalServiceContractForSignatureCommand c,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(c.EngagementId,false,ct);
        if(engagement is null||engagement.OrganizationId!=c.OrganizationId)
            return Result.Failure<ProfessionalServiceContractSnapshot>(ProfessionalEngagementErrors.NotFound);

        return await contracts.SendForSignatureAsync(
            engagement.Id,c.ActorUserId,ct);
    }
}

public sealed class RecordProfessionalServiceContractSignatureCommandHandler(
    IProfessionalEngagementRepository engagements,
    IProfessionalServiceContractGateway contracts)
    :ICommandHandler<RecordProfessionalServiceContractSignatureCommand,ProfessionalServiceContractSnapshot>
{
    public async Task<Result<ProfessionalServiceContractSnapshot>> Handle(
        RecordProfessionalServiceContractSignatureCommand c,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(c.EngagementId,false,ct);
        if(engagement is null||engagement.OrganizationId!=c.OrganizationId)
            return Result.Failure<ProfessionalServiceContractSnapshot>(ProfessionalEngagementErrors.NotFound);

        return await contracts.RecordSignatureAsync(new(
            engagement.Id,c.SignatoryPersonId,c.DocumentSha256,c.SignatureMethod,
            c.AuthenticationMethod,c.Provider,c.ProviderReference,c.CertificateReference,
            c.IpAddress,c.SignedAtUtc,c.ActorUserId),ct);
    }
}

public sealed class TerminateProfessionalServiceContractCommandHandler(
    IProfessionalEngagementRepository engagements,
    IProfessionalServiceContractGateway contracts,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock)
    :ICommandHandler<TerminateProfessionalServiceContractCommand>
{
    public async Task<Result> Handle(TerminateProfessionalServiceContractCommand c,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(c.EngagementId,true,ct);
        if(engagement is null||engagement.OrganizationId!=c.OrganizationId)
            return Result.Failure(ProfessionalEngagementErrors.NotFound);

        Result terminated=await contracts.TerminateAsync(engagement.Id,c.Reason,c.ActorUserId,ct);
        if(terminated.IsFailure)return terminated;

        Result unprepared=engagement.MarkPreparation(
            EngagementPreparationStep.Contract,false,clock.UtcNow,c.ActorUserId);
        if(unprepared.IsFailure)return unprepared;

        if(engagement.Status==ProfessionalEngagementStatus.Active)
        {
            Result suspended=engagement.Suspend("Professional service contract terminated",clock.UtcNow,c.ActorUserId);
            if(suspended.IsFailure)return suspended;
        }

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class PrepareProfessionalEngagementContractCommandHandler(
    IProfessionalEngagementRepository engagements,
    IProfessionalServiceContractGateway contracts,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock)
    :ICommandHandler<PrepareProfessionalEngagementContractCommand,ProfessionalServiceContractSnapshot>
{
    public async Task<Result<ProfessionalServiceContractSnapshot>> Handle(
        PrepareProfessionalEngagementContractCommand c,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(c.Id,true,ct);
        if(engagement is null||engagement.OrganizationId!=c.OrganizationId)
            return Result.Failure<ProfessionalServiceContractSnapshot>(ProfessionalEngagementErrors.NotFound);

        ProfessionalServiceContractSnapshot? contract=await contracts.GetByEngagementAsync(engagement.Id,ct);
        if(contract is null)
            return Result.Failure<ProfessionalServiceContractSnapshot>(
                ProfessionalEngagementErrors.SignedProfessionalContractRequired);

        if(!string.Equals(contract.Status,"Signed",StringComparison.OrdinalIgnoreCase))
            return Result.Failure<ProfessionalServiceContractSnapshot>(
                ProfessionalEngagementErrors.SignedProfessionalContractRequired);

        Result marked=engagement.MarkPreparation(
            EngagementPreparationStep.Contract,true,clock.UtcNow,c.ActorUserId);
        if(marked.IsFailure)
            return Result.Failure<ProfessionalServiceContractSnapshot>(marked.Error);

        await uow.CommitAsync(ct);
        return Result.Success(contract);
    }
}

public sealed class PrepareProfessionalEngagementComplianceCommandHandler(
    IProfessionalEngagementRepository engagements,
    IProfessionalProfileRepository profiles,
    IProfessionalMarketplaceUnitOfWork uow,
    IClock clock)
    :ICommandHandler<PrepareProfessionalEngagementComplianceCommand>
{
    public async Task<Result> Handle(
        PrepareProfessionalEngagementComplianceCommand c,CancellationToken ct)
    {
        ProfessionalEngagement? engagement=await engagements.GetAsync(c.Id,true,ct);
        if(engagement is null||engagement.OrganizationId!=c.OrganizationId)
            return Result.Failure(ProfessionalEngagementErrors.NotFound);

        ProfessionalProfile? profile=await profiles.GetByIdAsync(engagement.ProfessionalProfileId,ct);
        if(profile is null)
            return Result.Failure(ProfessionalProfileErrors.NotFound);

        if(profile.ComplianceStatus!=ProfessionalComplianceStatus.Compliant||
           profile.Status!=ProfessionalProfileStatus.Active)
            return Result.Failure(ProfessionalEngagementErrors.CompliantActiveProfessionalRequired);

        Result marked=engagement.MarkPreparation(
            EngagementPreparationStep.Compliance,true,clock.UtcNow,c.ActorUserId);
        if(marked.IsFailure)return marked;

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
