using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Domain.ProfessionalServiceContracts;
using DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.Contracts;

internal sealed class ProfessionalServiceContractGateway(
    IProfessionalServiceContractRepository contracts,
    IContractsUnitOfWork uow):IProfessionalServiceContractGateway
{
    public async Task<Result<ProfessionalServiceContractSnapshot>> CreateAsync(
        ProfessionalServiceContractCreationRequest request,
        CancellationToken cancellationToken=default)
    {
        if(await contracts.ExistsForEngagementAsync(request.EngagementId,cancellationToken))
            return Result.Failure<ProfessionalServiceContractSnapshot>(ProfessionalServiceContractErrors.Duplicate);

        ProfessionalServiceContractSignatory[] signatories=request.Signatories
            .Select(x=>new ProfessionalServiceContractSignatory(
                x.PersonId,x.Role,x.SigningOrder,x.IsRequired))
            .ToArray();

        Result<ProfessionalServiceContract> created=ProfessionalServiceContract.Create(
            request.Id,request.OrganizationId,request.EngagementId,request.ProfessionalProfileId,
            request.ProviderOrganizationId,request.ContractNumber,request.ContractType,
            request.SignatureOrder==ProfessionalContractSignatureOrder.Parallel
                ?ProfessionalServiceContractSignatureOrder.Parallel
                :ProfessionalServiceContractSignatureOrder.Sequential,
            request.TermsSnapshotJson,signatories,DateTimeOffset.UtcNow,request.ActorUserId);

        if(created.IsFailure)
            return Result.Failure<ProfessionalServiceContractSnapshot>(created.Error);

        contracts.Add(created.Value);
        await uow.CommitAsync(cancellationToken);
        return Result.Success(Map(created.Value));
    }

    public async Task<ProfessionalServiceContractSnapshot?> GetByEngagementAsync(
        ProfessionalEngagementId engagementId,
        CancellationToken cancellationToken=default)
    {
        ProfessionalServiceContract? contract=await contracts.GetByEngagementAsync(
            engagementId,false,cancellationToken);
        return contract is null?null:Map(contract);
    }

    public async Task<Result<ProfessionalServiceContractSnapshot>> GenerateAsync(
        ProfessionalEngagementId engagementId,
        string documentReference,
        string documentSha256,
        UserId actorUserId,
        CancellationToken cancellationToken=default)
    {
        ProfessionalServiceContract? contract=await contracts.GetByEngagementAsync(
            engagementId,true,cancellationToken);
        if(contract is null)
            return Result.Failure<ProfessionalServiceContractSnapshot>(ProfessionalServiceContractErrors.NotFound);

        Result generated=contract.Generate(documentReference,documentSha256,DateTimeOffset.UtcNow,actorUserId);
        if(generated.IsFailure)
            return Result.Failure<ProfessionalServiceContractSnapshot>(generated.Error);

        await uow.CommitAsync(cancellationToken);
        return Result.Success(Map(contract));
    }

    public async Task<Result<ProfessionalServiceContractSnapshot>> CreateRevisionAsync(
        ProfessionalEngagementId engagementId,
        string documentReference,
        string documentSha256,
        string reason,
        UserId actorUserId,
        CancellationToken cancellationToken=default)
    {
        ProfessionalServiceContract? contract=await contracts.GetByEngagementAsync(
            engagementId,true,cancellationToken);
        if(contract is null)
            return Result.Failure<ProfessionalServiceContractSnapshot>(ProfessionalServiceContractErrors.NotFound);

        Result revised=contract.CreateRevision(
            documentReference,documentSha256,reason,DateTimeOffset.UtcNow,actorUserId);
        if(revised.IsFailure)
            return Result.Failure<ProfessionalServiceContractSnapshot>(revised.Error);

        await uow.CommitAsync(cancellationToken);
        return Result.Success(Map(contract));
    }

    public async Task<Result<ProfessionalServiceContractSnapshot>> SendForSignatureAsync(
        ProfessionalEngagementId engagementId,
        UserId actorUserId,
        CancellationToken cancellationToken=default)
    {
        ProfessionalServiceContract? contract=await contracts.GetByEngagementAsync(
            engagementId,true,cancellationToken);
        if(contract is null)
            return Result.Failure<ProfessionalServiceContractSnapshot>(ProfessionalServiceContractErrors.NotFound);

        Result sent=contract.SendForSignature(DateTimeOffset.UtcNow,actorUserId);
        if(sent.IsFailure)
            return Result.Failure<ProfessionalServiceContractSnapshot>(sent.Error);

        await uow.CommitAsync(cancellationToken);
        return Result.Success(Map(contract));
    }

    public async Task<Result<ProfessionalServiceContractSnapshot>> RecordSignatureAsync(
        ProfessionalServiceContractSignatureRequest request,
        CancellationToken cancellationToken=default)
    {
        ProfessionalServiceContract? contract=await contracts.GetByEngagementAsync(
            request.EngagementId,true,cancellationToken);
        if(contract is null)
            return Result.Failure<ProfessionalServiceContractSnapshot>(ProfessionalServiceContractErrors.NotFound);

        Result recorded=contract.RecordSignature(
            request.SignatoryPersonId,request.DocumentSha256,request.SignatureMethod,
            request.AuthenticationMethod,request.Provider,request.ProviderReference,
            request.CertificateReference,request.IpAddress,request.SignedAtUtc,
            DateTimeOffset.UtcNow,request.ActorUserId);

        if(recorded.IsFailure)
            return Result.Failure<ProfessionalServiceContractSnapshot>(recorded.Error);

        await uow.CommitAsync(cancellationToken);
        return Result.Success(Map(contract));
    }

    public async Task<Result> TerminateAsync(
        ProfessionalEngagementId engagementId,
        string reason,
        UserId actorUserId,
        CancellationToken cancellationToken=default)
    {
        ProfessionalServiceContract? contract=await contracts.GetByEngagementAsync(
            engagementId,true,cancellationToken);
        if(contract is null)
            return Result.Failure(ProfessionalServiceContractErrors.NotFound);

        Result terminated=contract.Terminate(reason,DateTimeOffset.UtcNow,actorUserId);
        if(terminated.IsFailure)return terminated;

        await uow.CommitAsync(cancellationToken);
        return Result.Success();
    }

    private static ProfessionalServiceContractSnapshot Map(ProfessionalServiceContract x)
    {
        int required=x.Signatories.Count(s=>s.IsRequired);
        int signed=x.Signatories.Count(s=>s.IsRequired&&s.SignedAtUtc is not null);
        ProfessionalServiceContractSignatorySnapshot[] signatories=x.Signatories
            .Select(s=>new ProfessionalServiceContractSignatorySnapshot(
                s.PersonId.Value,s.Role,s.SigningOrder,s.IsRequired,s.SignedAtUtc,s.ReceivedAtUtc,
                s.SignatureMethod,s.AuthenticationMethod,s.Provider,s.ProviderReference,s.CertificateReference))
            .ToArray();

        ProfessionalServiceContractVersionSnapshotView[] versions=x.PreviousVersions
            .OrderByDescending(v=>v.Version)
            .Select(v=>new ProfessionalServiceContractVersionSnapshotView(
                v.Version,v.DocumentReference,v.DocumentSha256,v.Status.ToString(),v.GeneratedAtUtc,
                v.SentForSignatureAtUtc,v.SignedAtUtc,v.RevisionReason,v.SupersededAtUtc,v.SupersededByUserId.Value))
            .ToArray();

        return new(
            x.Id.Value,x.EngagementId.Value,x.ContractNumber,x.ContractType,x.Version,
            x.Status.ToString(),x.SignatureOrder.ToString(),x.DocumentReference,x.DocumentSha256,
            x.GeneratedAtUtc,x.SentForSignatureAtUtc,x.SignedAtUtc,x.TerminatedAtUtc,x.TerminationReason,
            required,signed,signatories,versions);
    }
}
