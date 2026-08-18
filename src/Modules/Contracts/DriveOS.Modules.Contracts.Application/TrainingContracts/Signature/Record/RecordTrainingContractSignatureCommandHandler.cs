using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Domain.SignatureProcesses;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Signature.Record;

public sealed class RecordTrainingContractSignatureCommandHandler(
    ITrainingContractRepository contracts,
    ISignatureProcessRepository signatureProcesses,
    IContractsUnitOfWork uow,
    IClock clock) : ICommandHandler<RecordTrainingContractSignatureCommand, RecordTrainingContractSignatureResponse>
{
    public async Task<Result<RecordTrainingContractSignatureResponse>> Handle(
        RecordTrainingContractSignatureCommand command,
        CancellationToken cancellationToken)
    {
        TrainingContract? contract = await contracts.GetByIdAsync(command.ContractId, cancellationToken);
        if (contract is null || contract.OrganizationId != command.OrganizationId)
            return Result.Failure<RecordTrainingContractSignatureResponse>(TrainingContractErrors.NotFound);

        SignatureProcess? process = await signatureProcesses.GetByIdAsync(command.SignatureProcessId, cancellationToken);
        if (process is null || process.OrganizationId != command.OrganizationId || process.ContractId != contract.Id)
            return Result.Failure<RecordTrainingContractSignatureResponse>(SignatureProcessErrors.NotFound);

        if (process.ContractVersionNumber != contract.CurrentVersionNumber)
            return Result.Failure<RecordTrainingContractSignatureResponse>(TrainingContractErrors.GeneratedDocumentOutdated);

        DateTimeOffset receivedAtUtc = clock.UtcNow;
        Result<SignatureEvidence> evidence = process.RecordSignature(
            command.SignatoryId,
            command.DocumentSha256,
            command.SignatureMethod,
            command.AuthenticationMethod,
            command.Provider,
            command.ProviderSignatureReference,
            command.CertificateReference,
            command.IpAddress,
            command.UserAgent,
            command.SignedAtUtc,
            receivedAtUtc,
            command.ActorUserId);

        if (evidence.IsFailure)
            return Result.Failure<RecordTrainingContractSignatureResponse>(evidence.Error);

        Result contractResult = contract.RecordSignatorySignature(
            command.SignatoryId,
            evidence.Value.Id,
            command.ActorUserId,
            evidence.Value.SignedAtUtc);

        if (contractResult.IsFailure)
            return Result.Failure<RecordTrainingContractSignatureResponse>(contractResult.Error);

        contract.SetModifiedAudit(receivedAtUtc, command.ActorUserId);
        await uow.CommitAsync(cancellationToken);

        return Result.Success(new RecordTrainingContractSignatureResponse(
            evidence.Value.Id.Value,
            command.SignatoryId.Value,
            process.Status.ToString(),
            contract.Status.ToString(),
            evidence.Value.SignedAtUtc));
    }
}
